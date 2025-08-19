using IDS.TextPlus.FCSEndpoint.Version;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint;

internal partial class Program
{
  private static readonly Dictionary<string, AbstractVersion> Versions = new()
  {
    { "1.2", new Version12() },
    { "2.0", new Version20() }
  };

  private static readonly AbstractVersion DefaultVersion = new Version20();

  private static void Main(string[] args)
  {
    short port = 16319;

    var server = new Server("*", port, ProcessRequest);
    server.AddEndpoint(HttpMethod.Post, "/fast", FastPost);

    Console.WriteLine($"SERVER STARTED on PORT:{port}");

    while (true) Thread.Sleep(500);
  }

  private static void ProcessRequest(HttpContext ctx)
  {
    var data = ctx.GetData();

    if (data.ContainsKey("version") && Versions.ContainsKey(data["version"]))
    {
      Versions[data["version"]].ProcessRequest(ctx, ref data);
      return;
    }
    if (data.ContainsKey("operation") && data["operation"] == "explain")
    {
      Versions["2.0"].ProcessRequest(ctx, ref data);
      return;
    }

    DefaultVersion.ProcessRequest(ctx, ref data);
  }
}