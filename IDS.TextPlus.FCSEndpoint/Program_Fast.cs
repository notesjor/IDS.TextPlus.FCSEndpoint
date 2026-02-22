using RestSharp;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint;

/// <summary>
///   This is only for IDS interal use only - this API is fast, simple and cames without the FCS bounderies
/// </summary>
internal partial class Program
{
  /// <summary>
  ///   MIME-Type for response
  /// </summary>
  private static readonly string _mime = "application/xml;charset=utf-8";

  private static readonly RestClient _client = new(new RestClientOptions { Timeout = new TimeSpan(0, 0, 15) });

  private static void FastPost(HttpContext context)
  {
    var post = context.Request.PostDataAsString;

    var request = new RestRequest("http://localhost:9200/fcs/_search", Method.Post);
    request.AddHeader("Content-Type", "application/json");
    request.AddStringBody(post, ContentType.Json);

    var response = _client.ExecuteAsync(request);
    response.Wait();

    context.Response.Send(response.Result.StatusCode, response.Result.Content, "application/json");
  }
}