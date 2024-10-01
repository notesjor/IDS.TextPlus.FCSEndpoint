using IDS.TextPlus.FCSEndpoint.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDS.TextPlus.FCSEndpoint.Helper
{
  public static class Search
  {
    private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
    {
      NullValueHandling = NullValueHandling.Ignore
    };
    private static RestClient _client = new RestClient(new RestClientOptions() { Timeout = new TimeSpan(0, 0, 10) });
    private static string _mime = "application/xml;charset=utf-8";

    public static SearchResponse Send(string query, int start, int maximum)
    {
      var request = new RestRequest("http://lexik08.ids-mannheim.de/meilisearch/indexes/fcs/search", Method.Post);
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Authorization", "Bearer 8jRAqq_GbtjdjveIOCxIlnztXjwFbcaMYp-e50HtbrQ");

      var obj = new SearchRequest
      {
        limit = maximum,
        offset = start - 1
      };

      obj.SetQuery(query);

      request.AddStringBody(JsonConvert.SerializeObject(obj, _serializerSettings), ContentType.Json);
      var response = _client.ExecuteAsync(request);
      response.Wait();

      return JsonConvert.DeserializeObject<SearchResponse>(response?.Result?.Content);
    }
  }
}
