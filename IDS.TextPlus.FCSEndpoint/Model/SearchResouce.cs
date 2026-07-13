using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResouce
{
  [JsonProperty("pid")] public string Pid { get; set; } = string.Empty;

  [JsonProperty("url")] public string Url { get; set; } = string.Empty;

  [JsonProperty("languages")] public string[] Languages { get; set; } = Array.Empty<string>();

  [JsonProperty("dataviews")] public string DataViews { get; set; } = string.Empty;

  [JsonProperty("lexfields")] public string LexFields { get; set; } = string.Empty;

  [JsonProperty("info")] public Dictionary<string, Dictionary<string, string>> Info { get; set; } = new();

  [JsonProperty("examples")] public Dictionary<string, string>[] Examples { get; set; } = Array.Empty<Dictionary<string, string>>();
}