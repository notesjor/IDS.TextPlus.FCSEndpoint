using System.Diagnostics;
using System.Linq;
using System.Text;
using IDS.TextPlus.FCSEndpoint.Model;
using Meilisearch;
using Newtonsoft.Json;
using Document = IDS.TextPlus.FCSEndpoint.Indexer.Model.Document;
using Index = Meilisearch.Index;

namespace IDS.TextPlus.FCSEndpoint.Indexer;

internal class Program
{
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
      if (doc.Related?.Count > 0)
      {
        var synonyms = doc.Related.Where(x => x.Type == "synonym").Select(x => x.Value).ToArray();
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
        Id = doc.Id.ToString(),
        Url = $"https://www.owid.de/artikel/{doc.Id}",
        Source = doc.Source,
        Text = stb.ToString(),
        Lemma = doc.Lemma,
        Pos = string.Join(" ", doc.Pos.Select(x => x.Value))
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

  private static void PrintInfo(Index index)
  {
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
    task = client.GetIndexAsync("fcs");
    task.Wait();

    var index = task.Result;
    index.UpdateSearchableAttributesAsync(new List<string> { "lemma", "def", "pos" }).Wait();
    index.UpdateFilterableAttributesAsync(new List<string> { "lemma", "source", "pos" }).Wait();

    index.UpdatePaginationAsync(new Pagination { MaxTotalHits = 10000 }).Wait();

    return index;
  }

  private static string Ask(string question)
  {
    Console.WriteLine(question);
    return Console.ReadLine();
  }
}