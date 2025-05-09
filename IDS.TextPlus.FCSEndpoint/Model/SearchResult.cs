using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResult
{
  [JsonProperty("source")] public string Source { get; set; }

  [JsonProperty("lemma")] public string Lemma { get; set; }

  [JsonProperty("id")] public string Id { get; set; }

  [JsonProperty("url")] public string Url { get; set; }

  [JsonProperty("text")] public string Text { get; set; }

  [JsonProperty("lang")] public string Lang { get; set; }

  [JsonProperty("facet_gender")] public string FacetGender { get; set; }

  [JsonProperty("gender")] public IList<SimpleValue> Gender { get; set; }

  [JsonProperty("facet_number")] public string FacetNumber { get; set; }

  [JsonProperty("number")] public IList<SimpleValue> Number { get; set; }

  [JsonProperty("facet_pos")] public string FacetPos { get; set; }

  [JsonProperty("pos")] public IList<SimpleValue> Pos { get; set; }

  [JsonProperty("facet_related")] public string FacetRelated { get; set; }

  [JsonProperty("related")] public IList<Related> Related { get; set; }

  // [JsonProperty("facet_citation")] public string FacetCitation { get; set; }

  [JsonProperty("citation")] public IList<Citation> Citation { get; set; }
}