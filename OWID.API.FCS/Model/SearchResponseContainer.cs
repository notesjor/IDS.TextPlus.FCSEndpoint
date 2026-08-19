using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResponseContainer : SearchResult
{
  [JsonPropertyName("_formatted")] public SearchResult Formatted { get; set; }
}