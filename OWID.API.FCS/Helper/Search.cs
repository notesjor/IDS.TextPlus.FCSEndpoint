using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IDS.TextPlus.FCSEndpoint.Model;

namespace IDS.TextPlus.FCSEndpoint.Helper;

public static class Search
{
  private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

  private static readonly JsonSerializerOptions _serializerOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  };

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

#if DEBUG
    Console.WriteLine(obj.ToRequestJson());
#endif

    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:9200/fcs/_search")
    {
      Content = new StringContent(obj.ToRequestJson(), Encoding.UTF8, "application/json")
    };

    var response = _httpClient.SendAsync(httpRequest).GetAwaiter().GetResult();
    var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

    return MapElasticsearchResponse(content, start - 1);
  }

  /// <summary>
  ///   Maps the raw Elasticsearch JSON response to a <see cref="SearchResponse" />.
  /// </summary>
  private static SearchResponse? MapElasticsearchResponse(string? json, int offset)
  {
    if (string.IsNullOrWhiteSpace(json))
      return null;

    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    if (!root.TryGetProperty("hits", out var hitsElement))
      return null;

    int totalValue = 0;
    if (hitsElement.TryGetProperty("total", out var totalEl) && totalEl.ValueKind == JsonValueKind.Object)
    {
      if (totalEl.TryGetProperty("value", out var valueEl) && valueEl.ValueKind == JsonValueKind.Number && valueEl.TryGetInt32(out var v))
        totalValue = v;
    }

    var containers = new List<SearchResponseContainer>();
    if (hitsElement.TryGetProperty("hits", out var hitArray) && hitArray.ValueKind == JsonValueKind.Array)
    {
      foreach (var hit in hitArray.EnumerateArray())
      {
        if (!hit.TryGetProperty("_source", out var sourceToken))
          continue;

        var container = JsonSerializer.Deserialize<SearchResponseContainer>(sourceToken, _serializerOptions);
        if (container == null)
          continue;

        var formatted = JsonSerializer.Deserialize<SearchResult>(sourceToken, _serializerOptions) ?? new SearchResult();

        if (hit.TryGetProperty("highlight", out var highlight) && highlight.ValueKind == JsonValueKind.Object)
        {
          if (highlight.TryGetProperty("text", out var textHl) && textHl.ValueKind == JsonValueKind.Array)
          {
            var parts = textHl.EnumerateArray().Select(e => e.GetString() ?? string.Empty);
            var joined = string.Join(" ", parts.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(joined))
              formatted.Text = joined;
          }

          if (highlight.TryGetProperty("lemma", out var lemmaHl) && lemmaHl.ValueKind == JsonValueKind.Array)
          {
            var parts = lemmaHl.EnumerateArray().Select(e => e.GetString() ?? string.Empty);
            var joined = string.Join(" ", parts.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(joined))
              formatted.Lemma = joined;
          }
        }

        container.Formatted = formatted;
        containers.Add(container);
      }
    }

    return new SearchResponse
    {
      Hits = containers.ToArray(),
      EstimatedTotalHits = totalValue,
      Offset = offset,
      ProcessingTimeMs = root.TryGetProperty("took", out var tookEl) ? tookEl.ToString() : null
    };
  }
}