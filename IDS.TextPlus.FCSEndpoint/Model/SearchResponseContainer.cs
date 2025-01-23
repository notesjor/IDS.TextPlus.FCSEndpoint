using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResponseContainer
{
  [JsonProperty("_formatted")] public SearchResult Formatted { get; set; }
}