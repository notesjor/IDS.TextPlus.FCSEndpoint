using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class SimpleValue
{

  /// <summary>
  /// Examples: "https://universaldependencies.org/u/feat/Number.html"
  /// </summary>
  [JsonPropertyName("schema")]
  public string Schema { get; set; }

  /// <summary>
  /// Examples: "Sing", "Coll", "Ptan"
  /// </summary>
  [JsonPropertyName("value")]
  public string Value { get; set; }
}