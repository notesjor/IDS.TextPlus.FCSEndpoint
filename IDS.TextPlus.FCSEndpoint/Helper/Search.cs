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
    public static RestClient _client = new RestClient(new RestClientOptions() { MaxTimeout = 5000 });
    public static string _mime = "application/xml;charset=utf-8";

    public static SearchResponse Send(string query, int start, int maximum)
    {
      var request = new RestRequest("http://lexik08.ids-mannheim.de/meilisearch/indexes/fcs/search", Method.Post);
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Authorization", "Bearer 8jRAqq_GbtjdjveIOCxIlnztXjwFbcaMYp-e50HtbrQ");
      request.AddStringBody(JsonConvert.SerializeObject(new SearchRequest
      {
        q = query,
        limit = maximum,
        offset = start - 1
      }), ContentType.Json);
      var response = _client.ExecuteAsync(request);
      response.Wait();

      return JsonConvert.DeserializeObject<SearchResponse>(response?.Result?.Content);
    }
  }
}
