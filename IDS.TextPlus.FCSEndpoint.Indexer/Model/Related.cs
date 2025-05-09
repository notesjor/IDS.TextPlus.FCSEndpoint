using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class Related
{

  /// <summary>
  /// Examples: "synonym", "related", "hypernym", "hyponym", "antonym"
  /// </summary>
  [JsonProperty("type")]
  public string Type { get; set; }

  /// <summary>
  /// Examples: "https://www.owid.de/artikel/42248", "https://www.owid.de/artikel/63768", "https://www.owid.de/artikel/100008", "https://www.owid.de/artikel/170966", "https://www.owid.de/artikel/100009"
  /// </summary>
  [JsonProperty("target")]
  public string Target { get; set; }

  /// <summary>
  /// Examples: "Homepage", "Leitseite", "Internetstartseite", "Beachvolleyball", "Strandvolleyballanlage"
  /// </summary>
  [JsonProperty("value")]
  public string Value { get; set; }
}