using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchResult
{
  [JsonPropertyName("lemma")] public string Lemma { get; set; }
  
  [JsonPropertyName("id")] public string Id { get; set; }

  [JsonPropertyName("oid")] public string OId { get; set; }

  [JsonPropertyName("sid")] public string SId { get; set; }
  
  [JsonPropertyName("source")] public string Source { get; set; }    

  [JsonPropertyName("url")] public string Url { get; set; }

  [JsonPropertyName("text")] public string Text { get; set; }

  [JsonPropertyName("lang")] public string Lang { get; set; }

  [JsonPropertyName("gender")] public string[] Gender { get; set; }

  [JsonPropertyName("gender_full")] public IList<SimpleValue> GenderFull { get; set; }

  [JsonPropertyName("number")] public string[] Number { get; set; }

  [JsonPropertyName("number_full")] public IList<SimpleValue> NumberFull { get; set; }

  [JsonPropertyName("pos")] public string[] Pos { get; set; }

  [JsonPropertyName("pos_full")] public IList<SimpleValue> PosFull { get; set; }

  [JsonPropertyName("citation")] public IList<Citation> Citation { get; set; }

  /// <summary>
  /// Link beinhaltet die Daten zu related, hyperonym, hyponym, antonym und synonym
  /// </summary>
  [JsonPropertyName("link")] public IList<Link> Link { get; set; }

  [JsonPropertyName("related")] public string[] Related { get; set; }

  [JsonPropertyName("hyperonym")] public string[] Hyperonym { get; set; }

  [JsonPropertyName("hyponym")] public string[] Hyponym { get; set; }

  [JsonPropertyName("antonym")] public string[] Antonym { get; set; }

  [JsonPropertyName("synonym")] public string[] Synonym { get; set; }
}