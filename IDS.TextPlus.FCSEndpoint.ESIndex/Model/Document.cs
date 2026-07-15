using System.Text.Json.Serialization;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model;

public class Document
{

  /// <summary>
  /// Examples: "sprw", "neo", "elex"
  /// </summary>
  [JsonPropertyName("source")]
  public string Source { get; set; }

  /// <summary>
  /// Examples: 401602, 401603, 401604, 100008, 100009
  /// </summary>
  [JsonPropertyName("id")]
  public string Id { get; set; }

  /// <summary>
  /// Examples: https://www.owid.de/artikel/666
  /// </summary>
  [JsonPropertyName("url")]
  public string Url { get; set; }

  /// <summary>
  /// Examples: "de"
  /// </summary>
  [JsonPropertyName("lang")]
  public string Lang { get; set; }

  /// <summary>
  /// Examples: 0, 1
  /// </summary>
  [JsonPropertyName("s_id")]
  public string SId { get; set; }

  /// <summary>
  /// Examples: "Das Auge isst mit.", "Die Augen sind der Spiegel der Seele.", "Das letzte Hemd hat keine Taschen.", "Startseite", "Strandvolleyball"
  /// </summary>
  [JsonPropertyName("lemma")]
  public string Lemma { get; set; }

  /// <summary>
  /// Examples: [], [], [], [{"schema":"https://grammis.ids-mannheim.de/terminologie","value":"nomen"},{"schema":"https://universaldependencies.org/u/feat/pos/","value":"NOUN"}], [{"schema":"https://grammis.ids-mannheim.de/terminologie","value":"nomen"},{"schema":"https://universaldependencies.org/u/feat/pos/","value":"NOUN"}]
  /// </summary>
  [JsonPropertyName("pos")]
  public IList<SimpleValue> Pos { get; set; }

  /// <summary>
  /// Examples: [{"id": 1, "text":""Sagt man dafür, dass auch das Aussehen und die Gestaltung von Speisen in einem entsprechenden Ambiente dazu beitragen, dass etwas als wohlschmeckend und appetitlich empfunden wird.", "Sagt man dafür, dass der Blick in die Augen eines Menschen Aufschluss über dessen Gefühlslage oder psychische Verfassung geben kann.", "Sagt man dafür, dass Reichtum oder materieller Besitz einem Menschen nach seinem Tod nichts mehr nützen und daher nicht überbewertet werden sollten.", "die erste Seite der Präsentation einer Person, Firma, Institution im Internet, die die wichtigsten Links zu weiterführenden Informationen enthält, Homepage", "Variante des Volleyballs, die von Zweiermannschaften auf Sand gespielt wird"}]
  /// </summary>
  [JsonPropertyName("def")]
  public IList<Definition> Def { get; set; }

  /// <summary>
  /// Examples: [{"defIf": 1, "example":"Das Auge isst mit Ein gutes Essen soll nicht nur den Gaumen verwöhnen – auch das Auge isst schließlich mit. Oft vergessen Gastgeber vor lauter Hektik am Herd jedoch, die Speisen ansehnlich anzurichten.","source":"Mannheimer Morgen, 04.10.2008, S. 5; Das Auge isst mit"},{"example":"\"Wenn der Tisch schön gedeckt ist, schmeckt es einfach besser. Meine Muttel sagte immer, ,das Auge ißt mit'.\"","source":"Frankfurter Rundschau, 18.04.1997, S. 22; Bei Dorothea Lucke in Preungesheim / Letzter Teil der FR-Serie über das Kochen"},{"example":"Nach dem Grundsatz \"Das Auge isst mit\" zeigen Firmen auch attraktives Verpackungsmaterial für die regionalen Leckereien. Dazu gehören Verpackungsmaschinen und Glasbehälter, in denen selbst erzeugte Produkte angeboten werden können.","source":"Nürnberger Nachrichten, 01.04.2000, S. 20; Über 200 Aussteller bei der \"Direkt-Markt\" in Nürnberg - Nahezu ausgebucht"}], [{"example":"Doch nicht nur das Lachen ist ein wichtiges Indiz fürs Glücklichsein, auch das Strahlen der Augen, wie 23 Prozent finden. »Die Augen sind der Spiegel der Seele«, weiß eine 19-jährige Gesamtschülerin.","source":"Nürnberger Zeitung, 09.03.2005; Umfrage: Kinder erkennen Glück am Lachen"},{"example":"Dass sich der 36-jährige Ami nicht so leicht einschüchtern lässt, davon hat sich Vitali in einem kurzen Gespräch mit dem siebenfachen Vater selbst überzeugt. \"Ich habe Tony in seinem Hotel getroffen. Die Augen sind Spiegel der Seele, und ich habe in seinen Augen weder Angst noch Respekt gesehen.\"","source":"Hamburger Morgenpost, 11.07.2008, S. 38; \"Wladimir haut ihn in der 5. Runde k.o.\""},{"example":"Die Augen sind, wie wir wissen, die Fenster zur Seele. Schauen wir also hinein, und es graust uns meist. Denn die Augen verraten, je nachdem, ob sie blitzen, funkeln oder trübe sind, die Geistesverfassung unseres Gegenüber.","source":"die tageszeitung, 23.07.1996, S. 10; Schau mir in die Augen"}], [{"example":"Ich war nie disziplinierbar, auch deshalb bin ich nicht käuflich. Geld interessiert mich nicht, auch deshalb habe ich finanzielle Probleme. Das letzte Hemd hat keine Taschen, sage ich immer, mitnehmen kann man eh nichts am letzten Tag.","source":"Rhein-Zeitung, 08.04.2006; Brinkmann: Kam mir vor wie ein Schwerverbrecher"},{"example":"Als Michael Kehr von der Finanzkrise erfuhr, sagte er sich: »Ich hab es genau richtig gemacht.« Sollen sich doch die anderen die Hacken ablaufen, sollen sie doch sagenhafte Reichtümer anhäufen, um sie am nächsten Tag zu verlieren, was juckt ihn das. »Der Spruch stimmt schon: Das letzte Hemd hat keine Taschen.« Kehr hat noch genau eine halbe Stunde, dann ist Feierabend. Pünktlich um ein Uhr mittags macht er Schluss, packt die Isomatte und den Bettelsack in den Rucksack, wirft den Pappbecher, der ihm als Abfalleimer dient, in den Müll und fährt nach Hause.","source":"Die Zeit (Online-Ausgabe), 23.10.2008, S. 6; Wo bitte geht's zum Untergang?"},{"example":"Zwar haben die meisten unter uns eine gewisse Schlagseite für Erfolg und nach materieller Absicherung - aber erinnern wir uns schon jetzt daran: Das letzte Hemd hat keine Taschen, was aber am inwendigen Menschen gewachsen ist, das bleibt bestehen.","source":"St. Galler Tagblatt, 19.08.1999; TT-EXT"}], [{"example":"In der Wahlnacht werden zusätzlich Hochrechnungen und das vorläufige Wahl- und Abstimmungsergebnis veröffentlicht. Die Startseite wird mit http://www.is.in-berlin.de/Wahl erreicht.","source":"die tageszeitung, 13.10.1995"},{"example":"Es spielt keine Rolle, an welchem Ort der Welt die digitalen Pavillons stehen. Von der Startseite der Weltausstellung aus sind sie alle bequem per Mausklick zu erreichen (http://park.org/).","source":"Die Zeit, 06/1996"},{"example":"Weil die Homepage der PDS eine Birne mit Gesicht ziert, wurde der Zugriff auf die Internet-Selbstdarstellung der Reformkommunisten über die Startseite des Bundestags kurzerhand gesperrt. Die Parlamentsverwaltung begründete die Zensur mit dem Hinweis, eine Birne würde auch zur karikierenden Darstellung von Kanzler Kohl verwendet.","source":"Computer Zeitung, 32/1997"},{"example":"Die Startseiten leiten den Online-Nutzer mit Links zu Suchfunktionen, Informationsangeboten, elektronischen Einkaufsmöglichkeiten oder zum Online-Plausch.","source":"Frankfurter Rundschau, 19.06.1998"},{"example":"Klickt man sich in das Programm hinein, wird man von einem nüchternen Inhaltsverzeichnis empfangen. Besonders aufwendig ist die Startseite nicht gestaltet, aber das ist ein Plus. Wichtiger als grafische Spielereien ist in diesem Fall die Übersichtlichkeit.","source":"Tages-Anzeiger, 19.06.1998"},{"example":"Berlin setzt Maßstäbe und zeigt, wie ein gelungener Auftritt aussehen kann. Schon die Startseite lädt durch freundliches Blau und modernes Design zum Stöbern im umfangreichen Angebot ein.","source":"Berliner Zeitung, 18.08.1999"},{"example":"Kundenbindende Service-Leistungen, wie etwa E-Mail, Einkaufsmöglichkeiten oder aktuelle Nachrichten, sorgen außerdem dafür, dass Nutzer immer wieder auf die Start-Seite zurückkommen. Für einige der großen Betreiber lohnt sich dieses Angebot bereits.","source":"Süddeutsche Zeitung, 21.12.1999"},{"example":"Wenn man auf dem Computer die Adresse der gewünschten Webseite eingibt (www.Gesobau.de), dann öffnet sich auf dem Bildschirm zunächst die Startseite der Wohnungsbaugesellschaft. Auf dieser Seite heißt die Gesobau alle Besucher willkommen.","source":"Berliner Zeitung, 20.01.2000"}], [{"example":"Schlesingers Team war zumindest in den letzten fünf Monaten ein Phantom. Ende März hat er die Spieler zum letzten gemeinsamen Training gebeten, seitdem gehen alle ihre eigenen Wege: in der Nationalmannschaft, in der Studentenauswahl oder beim sommerlichen Strandvolleyball, das neuerdings auch in Deutschland \"hip\" ist.","source":"die tageszeitung, 11.09.1993"},{"example":"Erst kürzlich waren die beiden zweimaligen Deutschen Meisterinnen im Strand-Volleyball als erste Nationalmannschaft für diese neue olympische Disziplin nominiert worden.","source":"die tageszeitung, 15.07.1994"},{"example":"\"Bin ich denn in Mannheim?\" fragte sich so mancher, der beim Spaziergang am Neckarufer sich an der Friedrich-Ebert-Brücke plötzlich am Strand unter Palmen und spärlich bekleideten Menschen wiederfand. Drei Tage lang hatte sich hier die Neckarwiese wegen des \"Beach Cup 95\" im Strandvolleyball in California verwandelt. 500 Tonnen Sand hatte der Deutsche Volleyballverband herankarren lassen, um die drei Beachfelder in der Mitte der Anlage strandgerecht auszustatten.","source":"Mannheimer Morgen, 17.07.1995"},{"example":"Die Zukunft des Beach-Handballs sieht Stefan Espenschied positiv. Der Sport könnte eine ähnliche Entwicklung wie die des Strand-Volleyballs nehmen, hofft er.","source":"Frankfurter Rundschau, 14.07.1998"},{"example":"Warum spielt Ihr denn Strandvolleyball - ist es langweilig in norddeutschen Sporthallen?","source":"die tageszeitung, 26.05.2000"}]
  /// </summary>
  [JsonPropertyName("citation")]
  public IList<Citation> Citation { get; set; }

  /// <summary>
  /// Examples: "Start|sei|te", "Strand|vol|ley|ball", "Street|wear", "Tha|las|so|the|ra|pie", "An|mel|de|un|ter|la|ge"
  /// </summary>
  [JsonPropertyName("segmentation")]
  public string Segmentation { get; set; }

  /// <summary>
  /// Examples: [{"schema":"https://grammis.ids-mannheim.de/systematische-grammatik/2263","value":"femininum"},{"schema":"https://universaldependencies.org/u/feat/Gender.html","value":"Fem"}], [{"schema":"https://grammis.ids-mannheim.de/systematische-grammatik/2263","value":"maskulinum"},{"schema":"https://universaldependencies.org/u/feat/Gender.html","value":"Masc"}], [{"schema":"https://grammis.ids-mannheim.de/systematische-grammatik/2263","value":"femininum"},{"schema":"https://universaldependencies.org/u/feat/Gender.html","value":"Fem"}], [{"schema":"https://grammis.ids-mannheim.de/systematische-grammatik/2263","value":"femininum"},{"schema":"https://universaldependencies.org/u/feat/Gender.html","value":"Fem"}], [{"schema":"https://grammis.ids-mannheim.de/systematische-grammatik/2263","value":"femininum"},{"schema":"https://universaldependencies.org/u/feat/Gender.html","value":"Fem"}]
  /// </summary>
  [JsonPropertyName("gender")]
  public IList<SimpleValue> Gender { get; set; }

  /// <summary>
  /// Examples: [{"schema":"https://universaldependencies.org/u/feat/Number.html","value":"Sing"}], [{"schema":"https://universaldependencies.org/u/feat/Number.html","value":"Coll"}], [{"schema":"https://universaldependencies.org/u/feat/Number.html","value":"Coll"}], [{"schema":"https://universaldependencies.org/u/feat/Number.html","value":"Sing"}], [{"schema":"https://universaldependencies.org/u/feat/Number.html","value":"Sing"}]
  /// </summary>
  [JsonPropertyName("number")]
  public IList<SimpleValue> Number { get; set; }

  /// <summary>
  /// Examples: [{"type":"link","target":"https://www.owid.de/artikel/42248","value":"Homepage"},{"type":"synonym","target":"https://www.owid.de/artikel/63768","value":"Leitseite"},{"type":"related","target":"https://www.owid.de/artikel/100008","value":"Internetstartseite"}], [{"type":"synonym","target":"https://www.owid.de/artikel/170966","value":"Beachvolleyball"},{"type":"related","target":"https://www.owid.de/artikel/100009","value":"Strandvolleyballanlage"},{"type":"related","target":"https://www.owid.de/artikel/100009","value":"Strandvolleyballer"},{"type":"related","target":"https://www.owid.de/artikel/100009","value":"Strandvolleyballfeld"},{"type":"related","target":"https://www.owid.de/artikel/100009","value":"Strandvolleyballturnier"}], [{"type":"related","target":"https://www.owid.de/artikel/101011","value":"Streetwearkollektion"},{"type":"related","target":"https://www.owid.de/artikel/101011","value":"Streetwearlabel"}], [{"type":"synonym","target":"https://www.owid.de/artikel/307995","value":"Thalasso"},{"type":"related","target":"https://www.owid.de/artikel/101352","value":"Thalassotherapieanwendung"},{"type":"related","target":"https://www.owid.de/artikel/101352","value":"Thalassotherapiezentrum"}]
  /// </summary>
  [JsonPropertyName("link")]
  public IList<Link> Link { get; set; }
}