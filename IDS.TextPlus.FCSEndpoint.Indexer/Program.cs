using System.Text;
using System.Reflection.Metadata;
using Meilisearch;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;

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

      var docs = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly)
        .SelectMany(file => JsonConvert.DeserializeObject<LDocument[]>(File.ReadAllText(file, Encoding.UTF8)));

      PushDocs(index, docs);
    }

    private static void PushDocs(Meilisearch.Index index, IEnumerable<LDocument> docs)
    {
      var tmp = new List<MDocument>();
      var max = 1000;

      foreach (var doc in docs)
      {
        for (var i = 0; i < doc.Pos.Length; i++)
          tmp.Add(new MDocument
          {
            // Bei diesem Mapping ist noch nicht klar, ob es so bleiben soll
            Id = ulong.Parse(doc.Href),
            Url = $"https://www.owid.de/artikel/{doc.Href}",

            // Bei diesem Mapping ist noch nicht klar, wie es indiziert werden soll
            Lemma = string.Join(" ", doc.Lemma),

            // Umbenennung
            Source = doc.Module,

            // Folgende Mappings sind eindeutig und klar
            Pos = doc.Pos[i],
            Def = doc.Def[i],

            No = i + 1
          });

        if (tmp.Count >= max)
        {
          index.AddDocumentsAsync(tmp).Wait();
          tmp.Clear();
        }
      }
      if (tmp.Count > 0)
        index.AddDocumentsAsync(tmp).Wait();
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

      index.UpdatePaginationAsync(new Pagination { MaxTotalHits = 1000 }).Wait();

      return index;
    }

    static string Ask(string question)
    {
      Console.WriteLine(question);
      return Console.ReadLine();
    }
  }
}
