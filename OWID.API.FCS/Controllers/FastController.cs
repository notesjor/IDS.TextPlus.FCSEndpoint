using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace OWID.API.FCS.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class FastController : Controller
  {
    private static HttpClient _client = new HttpClient();

    [HttpPost(Name = "GetFastResponse")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task Post()
    {
      try
      {
        using var content = new StreamContent(Request.Body);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        using var response = await _client.PostAsync("http://localhost:9200/fcs/_search", content);

        Response.StatusCode = (int)response.StatusCode;
        Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        await responseStream.CopyToAsync(Response.Body);
      }
      catch
      {
        Response.StatusCode = (int)HttpStatusCode.InternalServerError;        
      }
    }
  }
}
