using System.ComponentModel;
using System.Text.Json.Serialization;
using IDS.TextPlus.FCSEndpoint.Indexer.Model;

namespace IDS.TextPlus.FCSEndpoint.Model;

// Hinweise:
// - die doppelte Attributierung mit JsonProperty und JsonPropertyName ist notwendig, da die JsonSerializerSettings von Newtonsoft.Json und System.Text.Json unterschiedlich sind.
// - die Attribute sind notwendig, da die Attribute in der JSON nicht den C#-Konventionen entsprechen
// - Neue Attribute müssen auch in der FAST-Search (Program_Fast.cs) hinzugefügt werden.
// - Neue Attribute müssen ggf. auch in der Indexer-Logik (Indexer/Program.cs) hinzugefügt werden (was wird wie in Meilisearch indiziert).
// - Neue Attribute müssen ggf. auch in der Translator-Logik (TranslateFcs2Meilisearch) aufgenommen werden - insbesondere wenn es Facetten sind.
// - Neue Attribute müssen ggf. auch im catalog.json nachgetragen werden, damit sie via Explain angezeigt werden können.
// - Neue Attribute müssen ggf. auch in der SearchRequest unter searchableAttributes aufgenommen werden, damit sie in der Suche entsprechend ihrer Priorität berücksichtigt werden.

public class SearchResult
{
  [JsonPropertyName("lemma")] public string Lemma { get; set; }

  [JsonPropertyName("id")]public ulong Id { get; set; }

  [JsonPropertyName("entryId")] public string OId { get; set; }

  [JsonPropertyName("senseRef")] public string SId { get; set; }

  [JsonPropertyName("source")] public string Source { get; set; }

  [JsonPropertyName("url")] public string Url { get; set; }

  [JsonPropertyName("segmentation")] public string Segmentation { get; set; }

  [JsonPropertyName("definition")] public string Definition { get; set; }

  [JsonPropertyName("definitionStruct")] public IList<Definition> DefinitionStruct { get; set; }

  [JsonPropertyName("text")] public string Text { get; set; }

  [JsonPropertyName("lang")] public string Lang { get; set; }

  [JsonPropertyName("gender")] public string[] Gender { get; set; }  

  [JsonPropertyName("number")] public string[] Number { get; set; }

  [JsonPropertyName("pos")] public string[] Pos { get; set; }

  [JsonPropertyName("link")] public string[] Link { get; set; }

  [JsonPropertyName("hyperonym")] public string[] Hyperonym { get; set; }

  [JsonPropertyName("hyponym")] public string[] Hyponym { get; set; }

  [JsonPropertyName("antonym")] public string[] Antonym { get; set; }

  [JsonPropertyName("synonym")] public string[] Synonym { get; set; }

  /// <summary>
  /// FcsSnippet contains all the snippets for Lex-DataView
  /// </summary>
  [JsonPropertyName("fcs_snippet")] public Dictionary<string, string> FcsSnippets { get; set; }

  [JsonPropertyName("lemma_token")] public string[] LemmaTokens { get; set; }
  [JsonPropertyName("citation")] public string Citation { get; set; }
}