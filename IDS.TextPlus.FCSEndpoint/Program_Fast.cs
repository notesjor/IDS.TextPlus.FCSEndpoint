using IDS.TextPlus.FCSEndpoint.Model;
using Newtonsoft.Json;
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

  private static readonly JsonSerializerSettings _serializerSettings = new()
  {
    NullValueHandling = NullValueHandling.Ignore
  };

  private static void FastPost(HttpContext context)
  {
    var post = context.Request.PostData<SearchRequest>();
    post.highlightPreTag = "<hit>";
    post.highlightPostTag = "</hit>";

    var request = new RestRequest("http://lexik08.ids-mannheim.de/meilisearch/indexes/fcs/search", Method.Post);
    request.AddHeader("Content-Type", "application/json");
    request.AddHeader("Authorization", "Bearer 8jRAqq_GbtjdjveIOCxIlnztXjwFbcaMYp-e50HtbrQ");

    request.AddStringBody(JsonConvert.SerializeObject(post, _serializerSettings), ContentType.Json);

    var response = _client.ExecuteAsync(request);
    response.Wait();

    context.Response.Send(response.Result.StatusCode, response.Result.Content, _mime);
  }
}