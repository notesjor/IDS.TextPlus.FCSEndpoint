using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model
{
  public class Pos
  {

    /// <summary>
    /// Examples: "https://grammis.ids-mannheim.de/terminologie", "https://universaldependencies.org/u/feat/pos/"
    /// </summary>
    [JsonProperty("schema")]
    public string Schema { get; set; }

    /// <summary>
    /// Examples: "nomen", "NOUN"
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }
  }
}