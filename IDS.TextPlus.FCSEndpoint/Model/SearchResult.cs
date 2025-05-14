using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResult
{
  [JsonProperty("lemma")][JsonPropertyName("lemma")] public string Lemma { get; set; }

  [JsonProperty("id")][JsonPropertyName("id")] public string Id { get; set; }

  [JsonProperty("oid")][JsonPropertyName("oid")] public string OId { get; set; }

  [JsonProperty("sid")][JsonPropertyName("sid")] public string SId { get; set; }

  [JsonProperty("source")][JsonPropertyName("source")] public string Source { get; set; }

  [JsonProperty("url")][JsonPropertyName("url")] public string Url { get; set; }

  [JsonProperty("segmentation")][JsonPropertyName("segmentation")] public string Segmentation { get; set; }

  [JsonProperty("def")][JsonPropertyName("def")] public string Def { get; set; }

  [JsonProperty("text")][JsonPropertyName("text")] public string Text { get; set; }

  [JsonProperty("lang")][JsonPropertyName("lang")] public string Lang { get; set; }

  [JsonProperty("gender")][JsonPropertyName("gender")] public string[] Gender { get; set; }

  [JsonProperty("gender_full")][JsonPropertyName("gender_full")] public IList<SimpleValue> GenderFull { get; set; }

  [JsonProperty("number")][JsonPropertyName("number")] public string[] Number { get; set; }

  [JsonProperty("number_full")][JsonPropertyName("number_full")] public IList<SimpleValue> NumberFull { get; set; }

  [JsonProperty("pos")][JsonPropertyName("pos")] public string[] Pos { get; set; }

  [JsonProperty("pos_full")][JsonPropertyName("pos_full")] public IList<SimpleValue> PosFull { get; set; }

  [JsonProperty("citation")][JsonPropertyName("citation")] public IList<Citation> Citation { get; set; }

  /// <summary>
  /// Link beinhaltet die Daten zu related, hyperonym, hyponym, antonym und synonym
  /// </summary>
  [JsonProperty("link")][JsonPropertyName("link")] public IList<Link> Link { get; set; }

  [JsonProperty("related")][JsonPropertyName("related")] public string[] Related { get; set; }

  [JsonProperty("hyperonym")][JsonPropertyName("hyperonym")] public string[] Hyperonym { get; set; }

  [JsonProperty("hyponym")][JsonPropertyName("hyponym")] public string[] Hyponym { get; set; }

  [JsonProperty("antonym")][JsonPropertyName("antonym")] public string[] Antonym { get; set; }

  [JsonProperty("synonym")][JsonPropertyName("synonym")] public string[] Synonym { get; set; }
}