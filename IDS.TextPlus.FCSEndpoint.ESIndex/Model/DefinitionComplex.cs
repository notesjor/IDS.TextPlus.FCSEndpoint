using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class DefinitionComplex
{
  /// <summary>
  /// Examples: "Sagt man dafür, dass auch das Aussehen und die Gestaltung von Speisen in einem entsprechenden Ambiente dazu beitragen, dass etwas als wohlschmeckend und appetitlich empfunden wird.", "Sagt man dafür, dass der Blick in die Augen eines Menschen Aufschluss über dessen Gefühlslage oder psychische Verfassung geben kann.", "Sagt man dafür, dass Reichtum oder materieller Besitz einem Menschen nach seinem Tod nichts mehr nützen und daher nicht überbewertet werden sollten.", "die erste Seite der Präsentation einer Person, Firma, Institution im Internet, die die wichtigsten Links zu weiterführenden Informationen enthält, Homepage", "Variante des Volleyballs, die von Zweiermannschaften auf Sand gespielt wird"
  /// </summary>
  [JsonPropertyName("text")]
  public string Text { get; set; }

  /// <summary>
  /// Examples: 1, 2, 3, ...
  /// </summary>
  [JsonPropertyName("sid")]
  public string SId { get; set; }
}