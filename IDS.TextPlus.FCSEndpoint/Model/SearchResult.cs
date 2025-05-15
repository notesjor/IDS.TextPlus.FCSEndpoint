using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Model;

// Hinweise:
// - die doppelte Attributierung mit JsonProperty und JsonPropertyName ist notwendig, da die JsonSerializerSettings von Newtonsoft.Json und System.Text.Json unterschiedlich sind.
// - die Attribute sind notwendig, da die Attribute in der JSON nicht den C#-Konventionen entsprechen
// - Neue Attribute müssen auchin der FAST-Search (Program_Fast.cs) hinzugefügt werden.
// - Neue Attribute müssen ggf. auch in der Indexer-Logik (Indexer/Program.cs) hinzugefügt werden (was wird wie in Meilisearch indiziert).
// - Neue Attribute müssen ggf. auch in der Translator-Logik (TranslateFcs2Meilisearch) aufgenommen werden - insbesondere wenn es Facetten sind.
// - Neue Attribute müssen ggf. auch im catalog.json nachgetragen werden, damit sie via Explain angezeigt werden können.

public class SearchResult
{
  [JsonProperty("lemma")][JsonPropertyName("lemma")] public string Lemma { get; set; }

  [JsonProperty("lemma_fcs")][JsonPropertyName("lemma_fcs")] public string LammFcs { get; set; }

  [JsonProperty("id")][JsonPropertyName("id")] public string Id { get; set; }

  [JsonProperty("entryId")][JsonPropertyName("entryId")] public string OId { get; set; }

  [JsonProperty("senseRef")][JsonPropertyName("senseRef")] public string SId { get; set; }

  [JsonProperty("source")][JsonPropertyName("source")] public string Source { get; set; }

  [JsonProperty("url")][JsonPropertyName("url")] public string Url { get; set; }

  [JsonProperty("segmentation")][JsonPropertyName("segmentation")] public string Segmentation { get; set; }

  [JsonProperty("def")][JsonPropertyName("def")] public string Def { get; set; }

  [JsonProperty("text")][JsonPropertyName("text")] public string Text { get; set; }

  [JsonProperty("lang")][JsonPropertyName("lang")] public string Lang { get; set; }

  [JsonProperty("gender")][JsonPropertyName("gender")] public string[] Gender { get; set; }  

  [JsonProperty("number")][JsonPropertyName("number")] public string[] Number { get; set; }

  [JsonProperty("pos")][JsonPropertyName("pos")] public string[] Pos { get; set; }

  [JsonProperty("related")][JsonPropertyName("related")] public string[] Related { get; set; }

  [JsonProperty("hyperonym")][JsonPropertyName("hyperonym")] public string[] Hyperonym { get; set; }

  [JsonProperty("hyponym")][JsonPropertyName("hyponym")] public string[] Hyponym { get; set; }

  [JsonProperty("antonym")][JsonPropertyName("antonym")] public string[] Antonym { get; set; }

  [JsonProperty("synonym")][JsonPropertyName("synonym")] public string[] Synonym { get; set; }

  /// <summary>
  /// FcsSnippet contains all the snippets for Lex-DataView
  /// </summary>
  [JsonProperty("fcs_snippet")][JsonPropertyName("fcs_snippet")] public Dictionary<string, string> FcsSnippets { get; set; }
}