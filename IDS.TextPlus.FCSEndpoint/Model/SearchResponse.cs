using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResponse
{
  [JsonProperty("hits")] public SearchResponseContainer[] Hits { get; set; }

  [JsonProperty("query")] public string Query { get; set; }

  [JsonProperty("processingTimeMs")] public string ProcessingTimeMs { get; set; }

  [JsonProperty("limit")] public string Limit { get; set; }

  [JsonProperty("offset")] public int Offset { get; set; }

  [JsonProperty("estimatedTotalHits")] public int EstimatedTotalHits { get; set; }
}