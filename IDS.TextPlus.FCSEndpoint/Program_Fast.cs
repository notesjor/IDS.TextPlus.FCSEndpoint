using IDS.TextPlus.FCSEndpoint.Helper;
using IDS.TextPlus.FCSEndpoint.Model;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint
{
  /// <summary>
  /// This is only for IDS interal use only - this API is fast, simple and cames without the FCS bounderies  
  /// </summary>
  internal partial class Program
  {
    /// <summary>
    /// MIME-Type for response
    /// </summary>
    private static string _mime = "application/xml;charset=utf-8";
    private static RestClient _client = new RestClient(new RestClientOptions() { MaxTimeout = 5000 });
    private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
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
}
