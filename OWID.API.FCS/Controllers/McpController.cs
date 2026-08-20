using IDS.TextPlus.FCSEndpoint.Version;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace OWID.API.FCS.Controllers
{
  [ApiController]
  [Route("/mcp")]
  public class McpController : Controller
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

      HttpContext.Response.ContentType = "application/json";
      ((Version20)Versions["2.0"]).SendExplainBypass(HttpContext, ref data);
    }

    [HttpPost]
    public async Task Post()
    {
      var data = HttpContext.Request.Query
        .ToDictionary(q => q.Key?.ToLowerInvariant(), q => q.Value.ToString());

      HttpContext.Response.ContentType = "application/json";
      ((Version20)Versions["2.0"]).ProcessRequest(HttpContext, ref data);
    }
  }
}
