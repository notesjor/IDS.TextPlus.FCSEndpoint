using System.Text;
using System.Reflection.Metadata;
using HtmlAgilityPack;
using Meilisearch;
using System.Reflection.Metadata.Ecma335;

namespace IDS.TextPlus.FCSEndpoint.Indexer
{
  internal class Program
  {
    static void Main(string[] args)
    {
#if DEBUG
      var dir = @"C:\Users\Jan\Desktop\lex0";
      var meilisearchUrl = "http://lexik08.ids-mannheim.de:7700/";
      var meilisearchKey = "";
#else
      var dir = Ask("Enter the main-directory to index: ");
      var meilisearchUrl = Ask("Enter the MeiliSearch URL: ");
      var meilisearchKey = Ask("Enter the MeiliSearch key: ");
#endif

      var xml = new HtmlDocument(); // HtmlAgilityPack.HtmlDocument also reads XML
      var docs = new List<Document>();

      var client = new MeilisearchClient(meilisearchUrl, meilisearchKey);
      var index = EnsureIndex(client);
      var max = 1000;

      foreach (var file in Directory.EnumerateFiles(dir, "*.xml", SearchOption.AllDirectories))
      {
        docs.AddRange(ReadDocument(file, xml));
        if (docs.Count > max)
          PushDocs(index, ref docs);
      }

      if (docs.Count > 0)
        PushDocs(index, ref docs);
    }

    private static void PushDocs(Meilisearch.Index index, ref List<Document> docs)
    {
      index.AddDocumentsAsync(docs).Wait();
      docs.Clear();
    }

    private static Meilisearch.Index EnsureIndex(MeilisearchClient client)
    {
      client.CreateIndexAsync("fcs", "id").Wait();

      var task = client.GetIndexAsync("fcs");
      task.Wait();

      var index = task.Result;
      index.UpdateSearchableAttributesAsync(new List<string> { "lemma", "text" }).Wait();
      index.UpdateFilterableAttributesAsync(new List<string> { "source" }).Wait();

      return index;
    }

    private static IEnumerable<Document> ReadDocument(string file, HtmlDocument xml)
    {
      using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        xml.Load(fs, Encoding.UTF8);

      return ParseDocument(xml);
    }

    static int _id = 0;
    static IEnumerable<Document> ParseDocument(HtmlDocument xml)
    {
      foreach (var entry in xml.DocumentNode.SelectNodes("//entry"))
      {
        var lemma = entry.ChildNodes.FirstOrDefault(x => x.Name == "form" && x.GetAttributeValue("type", "") == "lemma")?.ChildNodes.FirstOrDefault(x => x.Name == "orth")?.InnerText;

        foreach (var sense in entry.SelectNodes("sense/def"))
        {
          yield return new Document
          {
            id = (_id++).ToString(),
            url = xml.DocumentNode.SelectSingleNode("//text/body/entry")?.GetAttributeValue("xml:id", "")?.Trim(),
            source = xml.DocumentNode.SelectSingleNode("//teiheader/filedesc/titlestmt/title")?.InnerText?.Trim(),

            lemma = lemma?.Trim(),
            text = sense.InnerText?.Trim().Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ")
          };
        }
      }
    }

    static string Ask(string question)
    {
      Console.WriteLine(question);
      return Console.ReadLine();
    }
  }
}
