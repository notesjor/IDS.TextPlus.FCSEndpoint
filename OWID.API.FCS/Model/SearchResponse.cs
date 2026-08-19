using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResponse
{
  [JsonPropertyName("hits")] public SearchResponseContainer[] Hits { get; set; }

  [JsonPropertyName("query")] public string Query { get; set; }

  [JsonPropertyName("processingTimeMs")] public string ProcessingTimeMs { get; set; }

  [JsonPropertyName("limit")] public string Limit { get; set; }

  [JsonPropertyName("offset")] public int Offset { get; set; }

  [JsonPropertyName("estimatedTotalHits")] public int EstimatedTotalHits { get; set; }
}