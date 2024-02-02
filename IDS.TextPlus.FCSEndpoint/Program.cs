using IDS.TextPlus.FCSEndpoint.Version;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint
{
  internal class Program
  {
    public static Dictionary<string, AbstractVersion> Versions = new Dictionary<string, AbstractVersion>();
    public static AbstractVersion DefaultVersion = new Version20();

    static void Main(string[] args)
    {
      short port = 16319;

      var server = new Server("*", port, (ctx) => DefaultVersion.ProcessRequest(ctx));
      Console.WriteLine($"SERVER STARTED on PORT:{port}");

      while (true)
      {
        Thread.Sleep(500);
      }
    }
  }
}
