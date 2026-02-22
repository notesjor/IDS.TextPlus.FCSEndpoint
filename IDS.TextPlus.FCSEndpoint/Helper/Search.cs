using IDS.TextPlus.FCSEndpoint.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace IDS.TextPlus.FCSEndpoint.Helper;

public static class Search
{
  private static readonly RestClient _client = new(new RestClientOptions { Timeout = new TimeSpan(0, 0, 10) });

  private static readonly JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings
  {
    NullValueHandling = NullValueHandling.Ignore
  });

  /// <summary>
  ///   Sends a LexCQL query to Elasticsearch and returns the result.
  /// </summary>
  public static SearchResponse? Send(string query, int start, int maximum, string? context = null)
  {
    var obj = new SearchRequest
    {
      Size = maximum,
      From = start - 1
    };

    obj.SetQuery(query);

    if (context != null)
    {
      var source = SearchResourceHelper.PidToKey.ContainsKey(context) ? SearchResourceHelper.PidToKey[context] : null;
      if (source == null && SearchResourceHelper.KeyToPid.ContainsKey(context))
        source = context;
      if (source != null)
        obj.AddSourceFilter(source);
    }

    var request = new RestRequest("http://localhost:9200/fcs/_search", Method.Post);
    request.AddHeader("Content-Type", "application/json");
    request.AddStringBody(obj.ToRequestJson(), ContentType.Json);
#if DEBUG
    Console.WriteLine(obj.ToRequestJson());
#endif

    var response = _client.ExecuteAsync(request);
    response.Wait();

    return MapElasticsearchResponse(response?.Result?.Content, start - 1);
  }

  /// <summary>
  ///   Maps the raw Elasticsearch JSON response to a <see cref="SearchResponse" />.
  /// </summary>
  private static SearchResponse? MapElasticsearchResponse(string? json, int offset)
  {
    if (string.IsNullOrWhiteSpace(json))
      return null;

    var esResponse = JObject.Parse(json);
    var hits = esResponse["hits"];
    if (hits == null)
      return null;

    var totalValue = hits["total"]?["value"]?.Value<int>() ?? 0;
    var hitArray = hits["hits"] as JArray;

    var containers = new List<SearchResponseContainer>();
    if (hitArray != null)
      foreach (var hit in hitArray)
      {
        var sourceToken = hit["_source"];
        if (sourceToken == null)
          continue;

        var container = sourceToken.ToObject<SearchResponseContainer>(_serializer);
        if (container == null)
          continue;

        // Build formatted version with ES highlighting
        var formatted = sourceToken.ToObject<SearchResult>(_serializer) ?? new SearchResult();
        var highlight = hit["highlight"] as JObject;
        if (highlight != null)
        {
          if (highlight["text"] is JArray textHl && textHl.Count > 0)
            formatted.Text = string.Join(" ", textHl.Select(t => t.ToString()));
          if (highlight["lemma"] is JArray lemmaHl && lemmaHl.Count > 0)
            formatted.Lemma = string.Join(" ", lemmaHl.Select(t => t.ToString()));
        }

        container.Formatted = formatted;
        containers.Add(container);
      }

    return new SearchResponse
    {
      Hits = containers.ToArray(),
      EstimatedTotalHits = totalValue,
      Offset = offset,
      ProcessingTimeMs = esResponse["took"]?.ToString()
    };
  }
}