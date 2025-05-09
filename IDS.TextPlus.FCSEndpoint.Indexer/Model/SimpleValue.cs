using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class SimpleValue
{

  /// <summary>
  /// Examples: "https://universaldependencies.org/u/feat/Number.html"
  /// </summary>
  [JsonProperty("schema")]
  public string Schema { get; set; }

  /// <summary>
  /// Examples: "Sing", "Coll", "Ptan"
  /// </summary>
  [JsonProperty("value")]
  public string Value { get; set; }
}