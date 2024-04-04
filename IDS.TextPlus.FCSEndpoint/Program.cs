using IDS.TextPlus.FCSEndpoint.Parser;
using IDS.TextPlus.FCSEndpoint.Version;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint
{
  internal partial class Program
  {
    private static Dictionary<string, AbstractVersion> Versions = new Dictionary<string, AbstractVersion>{
      { "1.2", new Version12() },
      { "2.0", new Version20() }
    };
    private static AbstractVersion DefaultVersion = new Version20();

    static void Main(string[] args)
    {
      short port = 16319;

      var server = new Server("*", port, ProcessRequest);
      server.AddEndpoint(HttpMethod.Post, "/fast", FastPost);

      Console.WriteLine($"SERVER STARTED on PORT:{port}");

      while (true)
      {
        Thread.Sleep(500);
      }
    }

    static void ProcessRequest(HttpContext ctx)
    {
      var data = ctx.GetData();

      if (data.ContainsKey("version") && Versions.ContainsKey(data["version"]))
      {
        Versions[data["version"]].ProcessRequest(ctx, ref data);
        return;
      }

      DefaultVersion.ProcessRequest(ctx, ref data);
    }
  }
}
