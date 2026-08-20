using IDS.TextPlus.FCSEndpoint.Version;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace OWID.API.FCS.Controllers
{
  [ApiController]
  [Route("/llm")]
  [Produces("application/json")]
  [McpServerToolType]
  public class McpController : ControllerBase
  {
    private static readonly Version20 DefaultVersion = new Version20();
    private static HttpClient _client = new HttpClient();

    [McpServerTool(Destructive = false, Name = "explain_data", OpenWorld = false, OutputSchemaType = typeof(explainResponse), ReadOnly = true, Title = "FCS-Explain Document")]
    [Description("Returns a JSON document that describes the FCS endpoint and its capabilities.")]
    [ProducesResponseType(typeof(explainResponse), 200)]
    public async Task<explainResponse> ExplainData()
    {
#if DEBUG
      Console.WriteLine("explain_data called");
#endif
      return DefaultVersion.SendExplainBypass();
    }

    [McpServerTool(Destructive = false, Name = "search_data", OpenWorld = false, OutputSchemaType = typeof(string), ReadOnly = true, Title = "FCS-Search v2.0")]
    [Description("You can use ElasticSearch queries to search for data in the FCS endpoint.")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<string> SearchData([Description("The ElasticSearch query to execute.")]string elasticSearchQuery)
    {
      try
      {
        using var content = new StringContent(elasticSearchQuery, Encoding.UTF8, "application/json");
#if DEBUG
        Console.WriteLine("search_data called");
        Console.WriteLine(elasticSearchQuery);
#endif
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        using var response = await _client.PostAsync("http://localhost:9200/fcs/_search", content);

#if DEBUG
        var res = await response.Content.ReadAsStringAsync();
        Console.WriteLine(res);
        return res;
#else
        return await response.Content.ReadAsStringAsync();
#endif

        return await response.Content.ReadAsStringAsync();
      }
      catch
      {
        return "Error occurred while executing the ElasticSearch query.";
      }
    }
  }
}
