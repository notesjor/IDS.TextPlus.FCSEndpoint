using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using IDS.TextPlus.FCSEndpoint.Model;
using Meilisearch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Document = IDS.TextPlus.FCSEndpoint.Indexer.Model.Document;
using Index = Meilisearch.Index;

namespace IDS.TextPlus.FCSEndpoint.Indexer;

internal class Program
{
  private static ulong _id = 0;

  private static void Main(string[] args)
  {
    var dir = GetClient(out var url, out var pass);

    var files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
    foreach (var file in files)
    {
      var docs = new List<Document>();

      try
      {
        var documentArray = JsonConvert.DeserializeObject<Document[]>(File.ReadAllText(file, Encoding.UTF8));
        docs.AddRange(documentArray);
      }
      catch // Temporärer Patch für Louis fehlerhafte Daten TODO
      {
        try
        {
          var documentArray = JsonConvert.DeserializeObject<Document[][]>(File.ReadAllText(file, Encoding.UTF8));
          foreach (var document in documentArray)
            docs.AddRange(document);
        }
        catch
        {
          // ignore
        }
      }

      Console.WriteLine($"Read {docs.Count} documents.");

      PushDocs(url, pass, new Queue<Document>(docs));
    }
  }

  private static string GetClient(out string url, out string pass)
  {
    var dir = Ask("Enter the main-directory to index: ");
    var meilisearchUrl = Ask("Enter the MeiliSearch URL: ");
    var meilisearchKey = Ask("Enter the MeiliSearch key: ");

    var client = new MeilisearchClient(meilisearchUrl, meilisearchKey);
    var index = EnsureIndex(client);
    url = $"{meilisearchUrl}indexes/fcs/documents";
    pass = meilisearchKey;

    return dir;
  }

  private static void PushDocs(string url, string pass, Queue<Document> docs)
  {
    Task<TaskInfo> task;
    var tmp = new List<SearchResult>();
    var max = 100;

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
        SendDocuments(url, pass, tmp);
        tmp.Clear();
        Console.WriteLine($"QUEUE: {docs.Count}");
      }
    }

    if (tmp.Count > 0)
    {
      SendDocuments(url, pass, tmp);
    }
  }

  private JsonSerializerSettings _settings = new JsonSerializerSettings
  {
    NullValueHandling = NullValueHandling.Ignore,
    Formatting = Newtonsoft.Json.Formatting.None,
  };

  private static void SendDocuments(string url, string pass, List<SearchResult> docs, int count = 5)
  {
    var client = new RestClient();
    var request = new RestRequest(url, Method.Post);
    request.AddHeader("Content-Type", "application/json");
    request.AddHeader("Authorization", $"Bearer {pass}");

    var json = JsonConvert.SerializeObject(docs, Newtonsoft.Json.Formatting.Indented);
    Console.WriteLine($"Sending: {json.Length / 1000} KB");
    request.AddStringBody(json, ContentType.Json);
    var response = client.ExecuteAsync(request);
    response.Wait();

    if (!response.Result.IsSuccessful)
    {
      if (count == 0)
      {
        Console.WriteLine($"Error: {response.Result.ErrorMessage} - No more retries left.");
        return;
      }

      Console.WriteLine($"Error: {response.Result.ErrorMessage} - Retrying {count} more times.");
      Thread.Sleep(60000);
      SendDocuments(url, pass, docs, count - 1);
    }
  }

  private static string[] _sentenceMarks = { " ", ".", "!", "?", ";", ":", ",", ")", "(", "[", "]", "\"", "'", "„", "“", "‘", "’", "-" };

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

  private static Index EnsureIndex(MeilisearchClient client)
  {
    client.CreateIndexAsync("fcs", "id").Wait();

    var task = client.GetIndexAsync("fcs");
    task.Wait();

    task.Result.DeleteAsync().Wait();

    client.CreateIndexAsync("fcs", "id").Wait();
    Thread.Sleep(2000);
    task = client.GetIndexAsync("fcs");
    task.Wait();

    var index = task.Result;
    // Bei Änderung von SearchableAttributes muss IDS.TextPlus.FCSEndpoint.Model.SearchRequest aktualisiert werden
    index.UpdateSearchableAttributesAsync(new List<string> { "lemma", "lemma_fcs", "related", "hyperonym", "hyponym", "antonym", "synonym", "definition", "citation" }).Wait();
    index.UpdateFilterableAttributesAsync(new List<string> { "entryId", "senseRef", "source", "number", "gender", "pos", "lang", "related", "hyperonym", "hyponym", "antonym", "synonym", "lemma_token" }).Wait();

    index.UpdatePaginationAsync(new Pagination { MaxTotalHits = 1000000 }).Wait();

    return index;
  }

  private static string Ask(string question)
  {
    Console.WriteLine(question);
    return Console.ReadLine();
  }
}