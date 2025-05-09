using IDS.TextPlus.FCSEndpoint.Model;
using Newtonsoft.Json;
using RestSharp;

namespace IDS.TextPlus.FCSEndpoint.Helper;

public static class Search
{
  private static readonly JsonSerializerSettings _serializerSettings = new()
  {
    NullValueHandling = NullValueHandling.Ignore
  };

  private static readonly RestClient _client = new(new RestClientOptions { Timeout = new TimeSpan(0, 0, 10) });
  private static string _mime = "application/xml;charset=utf-8";

  public static SearchResponse Send(string query, int start, int maximum, string? context = null)
  {
    var request = new RestRequest("http://lexik08.ids-mannheim.de/meilisearch/indexes/fcs/search", Method.Post);
    request.AddHeader("Content-Type", "application/json");
    request.AddHeader("Authorization", "Bearer 8jRAqq_GbtjdjveIOCxIlnztXjwFbcaMYp-e50HtbrQ");

    var obj = new SearchRequest
    {
      limit = maximum,
      offset = start - 1,
    };

    obj.SetQuery(query);
    if (context != null)
    {
      var source = SearchResourceHelper.PidToKey.ContainsKey(context) ? SearchResourceHelper.PidToKey[context] : null;
      if (source == null)
        if (SearchResourceHelper.KeyToPid.ContainsKey(context))
          source = context;
      if (source != null)
      {
        context = $"source = {source}";
        if (string.IsNullOrWhiteSpace(obj.filter))
          obj.filter = context;
        else
          obj.filter += " AND " + context;
      }
    }

    request.AddStringBody(JsonConvert.SerializeObject(obj, _serializerSettings), ContentType.Json);
    var response = _client.ExecuteAsync(request);
    response.Wait();

    return JsonConvert.DeserializeObject<SearchResponse>(response?.Result?.Content);
  }
}