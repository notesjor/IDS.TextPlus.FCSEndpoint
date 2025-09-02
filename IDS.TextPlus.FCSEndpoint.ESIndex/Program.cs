using Elastic.Clients.Elasticsearch;
using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using System.Net;
using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch.Tasks;
using IDS.TextPlus.FCSEndpoint.Model;

namespace IDS.TextPlus.FCSEndpoint.ESIndex
{
  internal class Program
  {
    private static ulong _id = 0;
    private static string _indexName = "fcs";
    private static string[] _sentenceMarks = { " ", ".", "!", "?", ";", ":", ",", ")", "(", "[", "]", "\"", "'", "„", "“", "‘", "’", "-" };

    private static void Main(string[] args)
    {
      var client = GetClient(out var dir);

      var files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
      foreach (var file in files)
      {
        var docs = new List<Document>();

        try
        {
          var documentArray = JsonSerializer.Deserialize<Document[]>(File.ReadAllText(file, Encoding.UTF8));
          docs.AddRange(documentArray);
        }
        catch // Temporärer Patch für Louis fehlerhafte Daten TODO
        {
          try
          {
            var documentArray = JsonSerializer.Deserialize<Document[][]>(File.ReadAllText(file, Encoding.UTF8));
            foreach (var document in documentArray)
              docs.AddRange(document);
          }
          catch
          {
            // ignore
          }
        }

        Console.WriteLine($"Read {docs.Count} documents.");

        PushDocs(client, new Queue<Document>(docs));
      }
    }

    private static ElasticsearchClient GetClient(out string dir)
    {
      dir = Ask("Enter the main-directory to index: ");

      var res = new ElasticsearchClient(new Uri("http://localhost:9200/"));
      EnsureIndex(res);

      return res;
    }

    private static void PushDocs(ElasticsearchClient client, Queue<Document> docs)
    {
      Task<TaskInfo> task;
      var tmp = new List<SearchResult>();
      var max = 10000;

      while (docs.Count > 0)
      {
        var doc = docs.Dequeue();

        var stb = new StringBuilder();
        stb.Append(doc.Lemma);
        if (!string.IsNullOrEmpty(doc.Segmentation))
          stb.Append(" (").Append(doc.Segmentation).Append(")");
        if (doc.Link?.Count > 0)
        {
          var synonyms = doc.Link.Where(x => x.Type == "synonym").Select(x => x.Value).ToArray();
          if (synonyms.Length > 0)
            stb.Append($" [{string.Join(", ", synonyms)}]");
        }

        if (doc.Pos?.Count > 0)
          stb.Append("; ").Append(string.Join(" / ", doc.Pos.Select(x => x.Value)));
        if (doc.Gender?.Count > 0)
          stb.Append(" (").Append(string.Join(" / ", doc.Gender.Select(x => x.Value))).Append(")");
        stb.Append($" - {doc.Def}");

        var snippetes = new Dictionary<string, string>
      {
        { "pos", GenerateSnippet(doc.Pos, "pos") },
        { "number", GenerateSnippet(doc.Number, "number") },
        { "gender", GenerateSnippet(doc.Gender, "gender") },
        { "related", GenerateSnippet(doc.Link?.Where(x => x.Type == "related")) },
        { "hyperonym", GenerateSnippet(doc.Link?.Where(x => x.Type == "hyperonym")) },
        { "hyponym", GenerateSnippet(doc.Link?.Where(x => x.Type == "hyponym")) },
        { "antonym", GenerateSnippet(doc.Link?.Where(x => x.Type == "antonym")) },
        { "synonym", GenerateSnippet(doc.Link?.Where(x => x.Type == "synonym")) },
        { "citation", GenerateSnippet(doc.Citation) },
        { "segmentation", doc.Segmentation == null ? "" : $"<lex:Field type=\"segmentation\">\r\n  <lex:Value>{doc.Segmentation}</lex:Value>\r\n</lex:Field>\r\n" },
        { "definition", doc.Def == null ? "" : $"<lex:Field type=\"definition\">\r\n  <lex:Value>{doc.Def}</lex:Value>\r\n</lex:Field>\r\n"}
      };

        tmp.Add(new SearchResult
        {
          Id = _id++,
          OId = doc.Id,
          SId = doc.SId,
          Segmentation = doc.Segmentation,
          Definition = doc.Def,
          Url = doc.Url,
          Source = doc.Source,
          Text = WebUtility.HtmlEncode(stb.ToString()),
          Lemma = WebUtility.HtmlEncode(doc.Lemma),
          LemmaTokens = Tokenize(doc.Lemma),
          LemmaFcs = doc.Segmentation == null ? doc.Lemma : string.Join(" ", doc.Segmentation.Split("|")),
          Gender = doc.Gender == null ? null : doc.Gender.Select(x => x.Value).ToArray(),
          Number = doc.Number == null ? null : doc.Number.Select(x => x.Value).ToArray(),
          Pos = doc.Pos == null ? null : doc.Pos.Select(x => x.Value).ToArray(),
          Lang = doc.Lang,
          Related = doc.Link?.Where(x => x.Type == "related")?.Select(x => x.Value)?.ToArray(),
          Hyperonym = doc.Link?.Where(x => x.Type == "hyperonym")?.Select(x => x.Value)?.ToArray(),
          Hyponym = doc.Link?.Where(x => x.Type == "hyponym")?.Select(x => x.Value)?.ToArray(),
          Antonym = doc.Link?.Where(x => x.Type == "antonym")?.Select(x => x.Value)?.ToArray(),
          Synonym = doc.Link?.Where(x => x.Type == "synonym")?.Select(x => x.Value)?.ToArray(),
          FcsSnippets = snippetes,
          Citation = doc.Citation == null ? null : string.Join(" ", doc.Citation.Select(x => x.Example)) // doc.Citation == null ? null : doc.Citation.First().Example //doc.Citation == null ? null : string.Join(" ", doc.Citation.Select(x=>x.Example))
        });

        if (tmp.Count >= max)
        {
          client.IndexMany(tmp, _indexName);
          tmp.Clear();
          Console.WriteLine($"QUEUE: {docs.Count}");
        }
      }

      if (tmp.Count > 0)
      {
        client.IndexMany(tmp, _indexName);
      }
    }

    private static string[] Tokenize(string docLemma)
    {
      return docLemma == null ? null : docLemma.Split(_sentenceMarks, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string GenerateSnippet(IEnumerable<Citation> values)
    {
      if (values == null || !values.Any())
        return string.Empty;

      var stb = new StringBuilder("<lex:Field type=\"citation\">");
      foreach (var x in values)
        stb.Append($"<lex:Value type=\"example\" source=\"{WebUtility.HtmlEncode(x.Source)}\">{WebUtility.HtmlEncode(x.Example)}</lex:Value>");
      stb.Append("</lex:Field>");

      return stb.ToString();
    }

    private static string GenerateSnippet(IEnumerable<Link>? values)
    {
      if (values == null || !values.Any())
        return string.Empty;

      var types = values.Select(x => x.Type).Distinct().ToArray();

      var stb = new StringBuilder();
      foreach (var type in types)
      {
        stb.Append($"<lex:Field type=\"{type}\">");
        foreach (var x in values)
          stb.Append($"<lex:Value ref=\"{x.Target}\">{x.Value}</lex:Value>");
        stb.Append("</lex:Field>");
      }

      return stb.ToString();
    }

    private static string GenerateSnippet(IEnumerable<SimpleValue>? values, string type)
    {
      if (values == null || !values.Any())
        return string.Empty;

      var stb = new StringBuilder($"<lex:Field type=\"{type}\">");
      foreach (var x in values)
        if (string.IsNullOrWhiteSpace(x.Schema))
          stb.Append($"<lex:Value>{x.Value}</lex:Value>");
        else
          stb.Append($"<lex:Value vocabValueRef=\"{x.Schema}\">{x.Value}</lex:Value>");
      stb.Append("</lex:Field>");

      return stb.ToString();
    }

    private static void EnsureIndex(ElasticsearchClient client)
    {
      if (client.Indices.Exists(_indexName).Exists)
      {
        client.Indices.Delete(_indexName);
        Thread.Sleep(2000);
      }

      var createIndexResponse = client.Indices.CreateAsync(_indexName, c => c
        .Mappings(m => m
          .Properties(p => p
            // Suchbare Felder (Text / Full-Text)
            .Text("lemma")
            .Text("lemma_fcs")
            .Text("related")
            .Text("hyperonym")
            .Text("hyponym")
            .Text("antonym")
            .Text("synonym")
            .Text("definition")
            .Text("citation")
            // Filterbare Felder (Keyword / exakter Wert)
            .Keyword("entryId")
            .Keyword("senseRef")
            .Keyword("source")
            .Keyword("number")
            .Keyword("gender")
            .Keyword("pos")
            .Keyword("lang")
            .Keyword("lemma_token")
          )
        )
        .Settings(s => s
            .NumberOfShards(1)
            .NumberOfReplicas(1)
        // Hier evtl. weitere Settings für Performance
        )
      );
      createIndexResponse.Wait();
    }

    private static string Ask(string question)
    {
      Console.WriteLine(question);
      return Console.ReadLine();
    }
  }
}
