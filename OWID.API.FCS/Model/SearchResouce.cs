using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResouce
{
  [JsonPropertyName("pid")] public string Pid { get; set; } = string.Empty;

  [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;

  [JsonPropertyName("languages")] public string[] Languages { get; set; } = Array.Empty<string>();

  [JsonPropertyName("dataviews")] public string DataViews { get; set; } = string.Empty;

  [JsonPropertyName("lexfields")] public string LexFields { get; set; } = string.Empty;

  [JsonPropertyName("info")] public Dictionary<string, Dictionary<string, string>> Info { get; set; } = new();

  [JsonPropertyName("examples")] public Dictionary<string, string>[] Examples { get; set; } = Array.Empty<Dictionary<string, string>>();
}