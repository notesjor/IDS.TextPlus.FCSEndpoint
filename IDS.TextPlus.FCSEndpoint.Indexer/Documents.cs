using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Indexer
{
  public class LDocument
  {
    [JsonProperty("lemma")]
    public string[] Lemma { get; set; }

    [JsonProperty("pos")]
    public string[] Pos { get; set; }

    [JsonProperty("def")]
    public string[] Def { get; set; }

    [JsonProperty("module")]
    public string Module { get; set; }

    [JsonProperty("href")]
    public string Href { get; set; }
  }

  public class MDocument
  {
    [JsonProperty("lemma")]
    public string Lemma { get; set; }

    [JsonProperty("pos")]
    public string Pos { get; set; }

    [JsonProperty("no")]
    public int No { get; set; }

    [JsonProperty("def")]
    public string Def { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("id")]
    public ulong Id { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
  }
}
