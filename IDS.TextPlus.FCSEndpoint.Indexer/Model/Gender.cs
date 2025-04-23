using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class Gender
{

  /// <summary>
  /// Examples: "https://grammis.ids-mannheim.de/systematische-grammatik/2263", "https://universaldependencies.org/u/feat/Gender.html"
  /// </summary>
  [JsonProperty("schema")]
  public string Schema { get; set; }

  /// <summary>
  /// Examples: "femininum", "Fem", "maskulinum", "Masc"
  /// </summary>
  [JsonProperty("value")]
  public string Value { get; set; }
}