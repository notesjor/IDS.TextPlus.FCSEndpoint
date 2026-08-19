using IDS.TextPlus.FCSEndpoint.Version;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace OWID.API.FCS.Controllers
{
  [ApiController]
  [Route("/")]
  public class FcsController : Controller
  {
    private static readonly Dictionary<string, AbstractVersion> Versions = new()
    {
      { "1.2", new Version12() },
      { "2.0", new Version20() }
    };

    private static readonly AbstractVersion DefaultVersion = new Version20();

    [HttpGet]
    public async Task Get()
    {
      var data = HttpContext.Request.Query
        .ToDictionary(q => q.Key?.ToLowerInvariant(), q => q.Value.ToString());

      HttpContext.Response.ContentType = "application/xml;charset=utf-8";

      if (data.ContainsKey("version") && Versions.ContainsKey(data["version"]))
      {
        Versions[data["version"]].ProcessRequest(HttpContext, ref data);
        return;
      }

      if (data.ContainsKey("operation") && data["operation"] == "explain")
      {
        Versions["2.0"].ProcessRequest(HttpContext, ref data);
        return;
      }

      DefaultVersion.ProcessRequest(HttpContext, ref data);
    }
  }
}
