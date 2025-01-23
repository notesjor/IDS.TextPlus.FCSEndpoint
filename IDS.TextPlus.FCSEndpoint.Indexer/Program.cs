using System.Diagnostics;
using System.Text;
using System.Reflection.Metadata;
using Meilisearch;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;
using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using IDS.TextPlus.FCSEndpoint.Model;

namespace IDS.TextPlus.FCSEndpoint.Indexer
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var dir = Ask("Enter the main-directory to index: ");
      var meilisearchUrl = Ask("Enter the MeiliSearch URL: ");
      var meilisearchKey = Ask("Enter the MeiliSearch key: ");

      var client = new MeilisearchClient(meilisearchUrl, meilisearchKey);
      var index = EnsureIndex(client);

      var docs = new List<Model.Document>();
      var files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly);
      foreach (var file in files)
      {
        var documentArray = JsonConvert.DeserializeObject<Model.Document[]>(File.ReadAllText(file, Encoding.UTF8));
        docs.AddRange(documentArray);
      }
      Console.WriteLine($"Read {docs.Count} documents.");

      PushDocs(index, docs);

      var count = index.GetStatsAsync();
      count.Wait();
      Console.WriteLine($"Indexed {count.Result.NumberOfDocuments} documents.");
    }

    private static void PushDocs(Meilisearch.Index index, IEnumerable<Model.Document> docs)
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
        if (doc.Xr != null)
        {
          var synonyms = doc.Xr.Where(x => x.Type == "synonym").Select(x => x.Value).ToArray();
          if (synonyms.Length > 0)
            stb.Append($" [{string.Join(", ", synonyms)}]");
        }
        if (!string.IsNullOrEmpty(doc.Pos))
          stb.Append("; ").Append(doc.Pos);
        if (!string.IsNullOrEmpty(doc.Gender))
          stb.Append(" (").Append(doc.Gender).Append(")");
        stb.Append($" - {doc.Def}");

        tmp.Add(new SearchResult
        {
          Id = doc.Id,
          Url = $"https://www.owid.de/artikel/{doc.Id}",
          Source = doc.Source,
          Text = stb.ToString(),
          Lemma = doc.Lemma,
          Pos = doc.Pos
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

    private static Meilisearch.Index EnsureIndex(MeilisearchClient client)
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
      index.UpdateFilterableAttributesAsync(new List<string> { "lemma", "source", "pos", }).Wait();

      index.UpdatePaginationAsync(new Pagination { MaxTotalHits = 10000 }).Wait();

      return index;
    }

    static string Ask(string question)
    {
      Console.WriteLine(question);
      return Console.ReadLine();
    }
  }
}
