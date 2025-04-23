using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class Citation
{

  /// <summary>
  /// Examples: "Das Auge isst mit Ein gutes Essen soll nicht nur den Gaumen verwöhnen – auch das Auge isst schließlich mit. Oft vergessen Gastgeber vor lauter Hektik am Herd jedoch, die Speisen ansehnlich anzurichten.", "\"Wenn der Tisch schön gedeckt ist, schmeckt es einfach besser. Meine Muttel sagte immer, ,das Auge ißt mit'.\"", "Nach dem Grundsatz \"Das Auge isst mit\" zeigen Firmen auch attraktives Verpackungsmaterial für die regionalen Leckereien. Dazu gehören Verpackungsmaschinen und Glasbehälter, in denen selbst erzeugte Produkte angeboten werden können.", "Doch nicht nur das Lachen ist ein wichtiges Indiz fürs Glücklichsein, auch das Strahlen der Augen, wie 23 Prozent finden. »Die Augen sind der Spiegel der Seele«, weiß eine 19-jährige Gesamtschülerin.", "Dass sich der 36-jährige Ami nicht so leicht einschüchtern lässt, davon hat sich Vitali in einem kurzen Gespräch mit dem siebenfachen Vater selbst überzeugt. \"Ich habe Tony in seinem Hotel getroffen. Die Augen sind Spiegel der Seele, und ich habe in seinen Augen weder Angst noch Respekt gesehen.\""
  /// </summary>
  [JsonProperty("example")]
  public string Example { get; set; }

  /// <summary>
  /// Examples: "Mannheimer Morgen, 04.10.2008, S. 5; Das Auge isst mit", "Frankfurter Rundschau, 18.04.1997, S. 22; Bei Dorothea Lucke in Preungesheim / Letzter Teil der FR-Serie über das Kochen", "Nürnberger Nachrichten, 01.04.2000, S. 20; Über 200 Aussteller bei der \"Direkt-Markt\" in Nürnberg - Nahezu ausgebucht", "Nürnberger Zeitung, 09.03.2005; Umfrage: Kinder erkennen Glück am Lachen", "Hamburger Morgenpost, 11.07.2008, S. 38; \"Wladimir haut ihn in der 5. Runde k.o.\""
  /// </summary>
  [JsonProperty("source")]
  public string Source { get; set; }
}