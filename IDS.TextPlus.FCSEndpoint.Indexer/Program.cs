namespace IDS.TextPlus.FCSEndpoint.Indexer
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var dir = Ask("Enter the directory to index: ");
      var meilisearchUrl = Ask("Enter the MeiliSearch URL: ");
      var meilisearchKey = Ask("Enter the MeiliSearch key: ");


    }

    static string Ask(string question)
    {
      Console.Write(question);
      return Console.ReadLine();
    }
  }
}
