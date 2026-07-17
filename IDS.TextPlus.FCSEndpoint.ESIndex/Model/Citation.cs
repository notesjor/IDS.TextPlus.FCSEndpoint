using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class Citation
{

  /// <summary>
  /// Examples: "In der Wahlnacht werden zusätzlich Hochrechnungen und das vorläufige Wahl- und Abstimmungsergebnis veröffentlicht. Die Startseite wird mit http://www.is.in-berlin.de/Wahl erreicht.", "Es spielt keine Rolle, an welchem Ort der Welt die digitalen Pavillons stehen. Von der Startseite der Weltausstellung aus sind sie alle bequem per Mausklick zu erreichen (http://park.org/).", "Weil die Homepage der PDS eine Birne mit Gesicht ziert, wurde der Zugriff auf die Internet-Selbstdarstellung der Reformkommunisten über die Startseite des Bundestags kurzerhand gesperrt. Die Parlamentsverwaltung begründete die Zensur mit dem Hinweis, eine Birne würde auch zur karikierenden Darstellung von Kanzler Kohl verwendet.", "Die Startseiten leiten den Online-Nutzer mit Links zu Suchfunktionen, Informationsangeboten, elektronischen Einkaufsmöglichkeiten oder zum Online-Plausch.", "Klickt man sich in das Programm hinein, wird man von einem nüchternen Inhaltsverzeichnis empfangen. Besonders aufwendig ist die Startseite nicht gestaltet, aber das ist ein Plus. Wichtiger als grafische Spielereien ist in diesem Fall die Übersichtlichkeit."
  /// </summary>
  [JsonPropertyName("example")]
  public string Example { get; set; }

  /// <summary>
  /// Examples: "die tageszeitung, 13.10.1995", "Die Zeit, 06/1996", "Computer Zeitung, 32/1997", "Frankfurter Rundschau, 19.06.1998", "Tages-Anzeiger, 19.06.1998"
  /// </summary>
  [JsonPropertyName("source")]
  public string Source { get; set; }

  /// <summary>
  /// Examples: 1, 2, 3, ...
  /// </summary>
  [JsonPropertyName("sid")]
  public string SId { get; set; } = "-";
}