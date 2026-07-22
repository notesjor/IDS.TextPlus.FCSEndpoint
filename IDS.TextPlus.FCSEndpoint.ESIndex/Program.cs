using Elastic.Clients.Elasticsearch;
using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using IDS.TextPlus.FCSEndpoint.Model;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

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

        PushDocs(client, new Queue<Document>(docs), file);
      }
    }

    private static ElasticsearchClient GetClient(out string dir)
    {
      dir = Ask("Enter the main-directory to index: ");

      var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200/"));

      var res = new ElasticsearchClient(settings);
      EnsureIndex(res);

      return res;
    }

    private static void PushDocs(ElasticsearchClient client, Queue<Document> docs, string file)
    {
      var tmp = new List<SearchResult>();
      var max = 10000;

      while (docs.Count > 0)
      {
        var doc = docs.Dequeue();

        tmp.Add(new SearchResult
        {
          Id = _id++,
          OId = doc.Id,
          Segmentation = doc.Segmentation,
          Definition = doc.Def is { Count: > 0 } ? "" : string.Join(" — ", doc.Def.Select(x => x.Text)),
          Url = doc.Url,
          Source = doc.Source,
          Text = GenerateSnippetSimpleText(doc),
          Lemma = doc.Lemma,
          LemmaTokens = Tokenize(doc.Lemma),
          Gender = doc.Gender == null ? null : doc.Gender.Select(x => x.Value).ToArray(),
          Number = doc.Number == null ? null : doc.Number.Select(x => x.Value).ToArray(),
          Pos = doc.Pos == null ? null : doc.Pos.Select(x => x.Value).ToArray(),
          Lang = doc.Lang,
          Link = doc.Link?.Where(x => x.Type == "link")?.Select(x => x.Value)?.ToArray(),
          Hyperonym = doc.Link?.Where(x => x.Type == "hyperonym")?.Select(x => x.Value)?.ToArray(),
          Hyponym = doc.Link?.Where(x => x.Type == "hyponym")?.Select(x => x.Value)?.ToArray(),
          Antonym = doc.Link?.Where(x => x.Type == "antonym")?.Select(x => x.Value)?.ToArray(),
          Synonym = doc.Link?.Where(x => x.Type == "synonym")?.Select(x => x.Value)?.ToArray(),
          FcsSnippets = GenerateSnippetFcs(doc),
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

    private static Dictionary<string, string> GenerateSnippetFcs(Document doc)
    {
      var snippetes = new Dictionary<string, string>
      {
        { "pos", GenerateSnippetFcsXml(doc.Pos, "pos") },
        { "number", GenerateSnippetFcsXml(doc.Number, "number") },
        { "gender", GenerateSnippetFcsXml(doc.Gender, "gender") },
        { "link", GenerateSnippetFcsXml(doc.Link?.Where(x => x.Type == "link")) },
        { "hyperonym", GenerateSnippetFcsXml(doc.Link?.Where(x => x.Type == "hyperonym")) },
        { "hyponym", GenerateSnippetFcsXml(doc.Link?.Where(x => x.Type == "hyponym")) },
        { "antonym", GenerateSnippetFcsXml(doc.Link?.Where(x => x.Type == "antonym")) },
        { "synonym", GenerateSnippetFcsXml(doc.Link?.Where(x => x.Type == "synonym")) },
        { "citation", GenerateSnippetFcsXml(doc.Citation) },
        { "segmentation", doc.Segmentation == null ? "" : $"<lex:Field type=\"segmentation\"><lex:Value>{doc.Segmentation}</lex:Value></lex:Field>" },
        { "definition", doc.Def is { Count: > 0 } ? "" : $"<lex:Field type=\"definition\">{string.Join("", doc.Def.Select(x=> $"<lex:Value xml:id=\"{x.SId}\">{x.Text}</lex:Value>"))}</lex:Field>"}
      };
      return snippetes;
    }

    private static string GenerateSnippetSimpleText(Document doc)
    {
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
      if(doc.Def is { Count: > 0 })
        stb.Append($" - {string.Join(" / ", doc.Def.Select(x=>x.Text))}");
      return HtmlEncoder.Default.Encode(stb.ToString());
    }

    private static string[] Tokenize(string docLemma)
    {
      return docLemma == null ? Array.Empty<string>() : docLemma.Split(_sentenceMarks, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string GenerateSnippetFcsXml(IEnumerable<Citation> values)
    {
      if (values == null || !values.Any())
        return string.Empty;

      var stb = new StringBuilder("<lex:Field type=\"citation\">");
      foreach (var x in values)
        stb.Append($"<lex:Value idRefs=\"{x.SId}\" type=\"example\" source=\"{(string.IsNullOrEmpty(x.Source) ? "" : HtmlEncoder.Default.Encode(x.Source))}\">{(string.IsNullOrEmpty(x.Example) ? "" : HtmlEncoder.Default.Encode(x.Example))}</lex:Value>");
      stb.Append("</lex:Field>");

      return stb.ToString();
    }

    private static string GenerateSnippetFcsXml(IEnumerable<Link>? values)
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

    private static string GenerateSnippetFcsXml(IEnumerable<SimpleValue>? values, string type)
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
      try
      {
        var deleteIndexResponse = client.Indices.Delete(_indexName);
        Thread.Sleep(2000);
        Console.WriteLine($"Index deletion response: {deleteIndexResponse.IsValidResponse}");
      }
      catch
      {
        // ignore
      }

      var createIndexResponse = client.Indices.Create(_indexName, c => c
        .Settings(s => s
          .NumberOfShards(1)
          .NumberOfReplicas(1)
          .Analysis(a => a
            .Tokenizers(to => to
              .Pattern("segmentation_tokenizer", pt => pt
                .Pattern("\\|")
              )
            )
            .Analyzers(an => an
              .Custom("segmentation_analyzer", ca => ca
                .Tokenizer("segmentation_tokenizer")
                .Filter("lowercase")
              )
              .Custom("lemma_analyzer", ca => ca
                .Tokenizer("standard")
                .Filter("lowercase")
              )
            )
          )
        )
        .Mappings(m => m
          .Properties(p => p
            // Suchbare Felder (Text / Full-Text)            
            .Text("definition")
            .Text("citation")
            .Text("text", k =>
              k.Index(false)
              .Store()
            )
            .Text("segmentation", k =>
            {
              k.Analyzer("segmentation_analyzer");
            })
            .Text("lemma", k => k
              .Analyzer("lemma_analyzer")
              .Fields(ff => ff
                .Wildcard("wildcard")
                .Keyword("keyword")
              )
            )
            // Filterbare Felder (Keyword / exakter Wert)            
            .Keyword("link")
            .Keyword("hyperonym")
            .Keyword("hyponym")
            .Keyword("antonym")
            .Keyword("synonym")
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
      );
      Console.WriteLine($"Index creation response: {createIndexResponse.IsValidResponse}");
    }

    private static string Ask(string question)
    {
      Console.WriteLine(question);
      return Console.ReadLine();
    }
  }
}
