using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using IDS.TextPlus.FCSEndpoint.Model;
using Meilisearch;
using Newtonsoft.Json;
using Document = IDS.TextPlus.FCSEndpoint.Indexer.Model.Document;
using Index = Meilisearch.Index;

namespace IDS.TextPlus.FCSEndpoint.Indexer;

internal class Program
{
  private static ulong _id = 0;

  private static void Main(string[] args)
  {
    var dir = GetClient(out var index);

    var docs = LoadLocalFiles(dir);

    PushDocs(index, docs);

    PrintInfo(index);
  }

  private static string GetClient(out Index index)
  {
    var dir = Ask("Enter the main-directory to index: ");
    var meilisearchUrl = Ask("Enter the MeiliSearch URL: ");
    var meilisearchKey = Ask("Enter the MeiliSearch key: ");

    var client = new MeilisearchClient(meilisearchUrl, meilisearchKey);
    index = EnsureIndex(client);
    return dir;
  }

  private static List<Document> LoadLocalFiles(string dir)
  {
    var docs = new List<Document>();
    var files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
    foreach (var file in files)
    {
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
    }

    Console.WriteLine($"Read {docs.Count} documents.");
    return docs;
  }

  private static void PushDocs(Index index, IEnumerable<Document> docs)
  {
    Task<TaskInfo> task;
    var tmp = new List<SearchResult>();
    var max = 10000;

    foreach (var doc in docs)
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
      stb.Append($" - {doc.Def}");

      tmp.Add(new SearchResult
      {
        Id = (_id++).ToString(),
        OId = doc.Id.ToString(),
        SId = doc.SId.ToString(),  
        Segmentation = doc.Segmentation,
        Def = doc.Def,
        Url = $"https://www.owid.de/artikel/{doc.Id}",
        Source = doc.Source,
        Text = WebUtility.HtmlEncode(stb.ToString()),
        Lemma = WebUtility.HtmlEncode(doc.Lemma),
        Gender = doc.Gender == null ? null : doc.Gender.Select(x => x.Value).ToArray(),
        GenderFull = doc.Gender,
        Number = doc.Number == null ? null :doc.Number.Select(x => x.Value).ToArray(),
        NumberFull = doc.Number,
        Pos = doc.Pos == null ? null :doc.Pos.Select(x => x.Value).ToArray(),
        PosFull = doc.Pos,        
        Citation = doc.Citation == null ? null : ProtectCitation(doc.Citation),
        Lang = doc.Lang,
        Link = doc.Link,
        Related = doc.Link?.Where(x => x.Type == "related")?.Select(x => x.Value)?.ToArray(),
        Hyperonym = doc.Link?.Where(x => x.Type == "hyperonym")?.Select(x => x.Value)?.ToArray(),
        Hyponym = doc.Link?.Where(x => x.Type == "hyponym")?.Select(x => x.Value)?.ToArray(),
        Antonym = doc.Link?.Where(x => x.Type == "antonym")?.Select(x => x.Value)?.ToArray(),
        Synonym = doc.Link?.Where(x => x.Type == "synonym")?.Select(x => x.Value)?.ToArray()
      });

      if (tmp.Count >= max)
      {
        task = index.AddDocumentsAsync(tmp);
        task.Wait();
        Debug.Write(task);
        tmp.Clear();
      }
    }

    if (tmp.Count > 0)
    {
      task = index.AddDocumentsAsync(tmp);
      task.Wait();
      Debug.Write(task);
    }
  }

  private static IList<Citation> ProtectCitation(IList<Citation> docCitation)
  {
    var res = new List<Citation>();
    foreach (var citation in docCitation)
    {
      res.Add(new Citation
      {
        Source = WebUtility.HtmlEncode(citation.Source),
        Example = WebUtility.HtmlEncode(citation.Example),
      });
    }
    return res;
  }

  private static void PrintInfo(Index index)
  {
    Thread.Sleep(2000);
    var count = index.GetStatsAsync();
    count.Wait();
    Console.WriteLine($"Indexed {count.Result.NumberOfDocuments} documents.");
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
    index.UpdateSearchableAttributesAsync(new List<string> { "lemma" }).Wait();
    index.UpdateFilterableAttributesAsync(new List<string> { "oid", "sid", "source", "number", "gender", "pos", "lang", "related", "hyperonym", "hyponym", "antonym", "synonym" }).Wait();

    index.UpdatePaginationAsync(new Pagination { MaxTotalHits = 1000000 }).Wait();

    return index;
  }

  private static string Ask(string question)
  {
    Console.WriteLine(question);
    return Console.ReadLine();
  }
}