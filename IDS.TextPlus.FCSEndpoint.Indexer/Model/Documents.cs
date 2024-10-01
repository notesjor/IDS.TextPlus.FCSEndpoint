using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Indexer.Model
{
  public class Cit
  {

    /// <summary>
    /// Examples: "Während der Fremdenverkehrsverband Liguriens lautstark \"Schluß mit dem Alarmismus und dem Katastrophengeschrei\" dekretiert, prophezeit gleichzeitig ein Sprecher der Fischer-Innung ein \"auf Jahre hinaus unbenutzbares Meer\".", "Insofern kann Klaus Hartung nicht mehr damit beruhigen, dem herrschenden Alarmismus den Stempel \"links\" aufzudrücken. Zumal es bei der intellektuellen Rechten die entsprechende Neigung gibt: Sie sehen eine \"Hegemonie der Linken\" in Deutschland und geben sich nicht minder alarmiert.", "Diese Passagen des Buches, die zu den stärksten gehören, atmen eine Gelassenheit, die sich wohltuend von dem schrillen Alarmismus abhebt, in den selbst viele Lehrer verfallen. Fels plädiert für eine Einheit von Unterrichten und Erziehen.", "Zunächst sollte man wohl den bei Fragen der öffentlichen Finanzen nahezu üblichen Alarmismus relativieren. Daß diese in einer kritischen Lage sind, ist gewiß kein aktuelles, sondern ein Dauer-Phänomen.", "Das sind, gerafft, Thesen aus den beiden Bänden Wilhelm Heitmeyers, in denen er mit dreißig Wissenschaftlern der großen Frage nachspürt, ob sich die Bundesrepublik auf dem Weg von der Konsens- zur Konfliktgesellschaft befinde. Ist das bloß Alarmismus? Mir scheint, nein."
    /// </summary>
    [JsonProperty("example")]
    public string Example { get; set; }

    /// <summary>
    /// Examples: "die tageszeitung, 22.04.1991", "die tageszeitung, 24.11.1992", "die tageszeitung, 01.10.1994", "Frankfurter Allgemeine, 1995", "Die Zeit, 30/1997"
    /// </summary>
    [JsonProperty("source")]
    public string Source { get; set; }
  }

  public class Xr
  {

    /// <summary>
    /// Examples: "synonym", "related"
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Examples: "https://www.owid.de/artikel/316400", "https://www.owid.de/artikel/401240", "https://www.owid.de/artikel/317335", "https://www.owid.de/artikel/317668", "https://www.owid.de/artikel/407497"
    /// </summary>
    [JsonProperty("target")]
    public string Target { get; set; }

    /// <summary>
    /// Examples: "elektronisches Papier", "facebooken", "twittern", "zwitschern", "youtuben"
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }
  }

  public class Document
  {

    /// <summary>
    /// Examples: "neo", "sprw"
    /// </summary>
    [JsonProperty("source")]
    public string Source { get; set; }

    /// <summary>
    /// Examples: 11967, 11968, 316400, 401184, 403700
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Examples: "de"
    /// </summary>
    [JsonProperty("lang")]
    public string Lang { get; set; }

    /// <summary>
    /// Examples: 0, 1, 2
    /// </summary>
    [JsonProperty("s_id")]
    public int SId { get; set; }

    /// <summary>
    /// Examples: "Alarmismus", "Alarmist", "E-Paper", "den X [Name einer Person] machen", "welchen Teil von X [Äußerung] versteht jemand nicht?"
    /// </summary>
    [JsonProperty("lemma")]
    public string Lemma { get; set; }

    /// <summary>
    /// Examples: "NOUN", "VERB", "X", "ADJ"
    /// </summary>
    [JsonProperty("pos")]
    public string Pos { get; set; }

    /// <summary>
    /// Examples: "Haltung, die sich in übertriebenen Warnungen vor sich abzeichnenden Fehlentwicklungen, Gefahren in der Gesellschaft äußert", "jemand, der in übertriebener Weise vor sich abzeichnenden Fehlentwicklungen, Gefahren in der Gesellschaft warnt", "Variante der gedruckten Ausgabe einer Zeitung oder Zeitschrift im Internet", "wiederaufladbares elektronisches Papier für das Lesen von Zeitungs- oder Buchinhalten", "eine Verhaltensweise zeigen, die der genannten prominenten Person als charakteristisch zugeschrieben wird"
    /// </summary>
    [JsonProperty("def")]
    public string Def { get; set; }

    /// <summary>
    /// Examples: "Alar|mis|mus", "Alar|mist", "E-Pa|per", "ins|ta|gram|men", "Fly|er"
    /// </summary>
    [JsonProperty("segmentation")]
    public string Segmentation { get; set; }

    /// <summary>
    /// Examples: "masc", "neut", "other", "fem"
    /// </summary>
    [JsonProperty("gender")]
    public string Gender { get; set; }

    /// <summary>
    /// Examples: [{"example":"Während der Fremdenverkehrsverband Liguriens lautstark \"Schluß mit dem Alarmismus und dem Katastrophengeschrei\" dekretiert, prophezeit gleichzeitig ein Sprecher der Fischer-Innung ein \"auf Jahre hinaus unbenutzbares Meer\".","source":"die tageszeitung, 22.04.1991"},{"example":"Insofern kann Klaus Hartung nicht mehr damit beruhigen, dem herrschenden Alarmismus den Stempel \"links\" aufzudrücken. Zumal es bei der intellektuellen Rechten die entsprechende Neigung gibt: Sie sehen eine \"Hegemonie der Linken\" in Deutschland und geben sich nicht minder alarmiert.","source":"die tageszeitung, 24.11.1992"},{"example":"Diese Passagen des Buches, die zu den stärksten gehören, atmen eine Gelassenheit, die sich wohltuend von dem schrillen Alarmismus abhebt, in den selbst viele Lehrer verfallen. Fels plädiert für eine Einheit von Unterrichten und Erziehen.","source":"die tageszeitung, 01.10.1994"},{"example":"Zunächst sollte man wohl den bei Fragen der öffentlichen Finanzen nahezu üblichen Alarmismus relativieren. Daß diese in einer kritischen Lage sind, ist gewiß kein aktuelles, sondern ein Dauer-Phänomen.","source":"Frankfurter Allgemeine, 1995"},{"example":"Das sind, gerafft, Thesen aus den beiden Bänden Wilhelm Heitmeyers, in denen er mit dreißig Wissenschaftlern der großen Frage nachspürt, ob sich die Bundesrepublik auf dem Weg von der Konsens- zur Konfliktgesellschaft befinde. Ist das bloß Alarmismus? Mir scheint, nein.","source":"Die Zeit, 30/1997"},{"example":"Freilich hat auch bei seiner [Sebastian Haffners] Sicht des Prügelfestes beim Schahbesuch seine Erfahrung der dreißiger Jahre in Berlin Pate gestanden. Er fühlte sich an Pogrome und SA-Prügeleien erinnert, spricht von der Berliner Blutnacht und fürchtet eine Refaschisierung der Bundesrepublik. Haffners Aufgerautheit speiste sich aus seiner Begegnung mit Willkür und hatte wenig gemein mit dem Alarmismus, der heutige Journalisten bei allen möglichen Anlässen umtreibt.","source":"Süddeutsche Zeitung, 22.12.1997"},{"example":"das Überleben der Erde und der Menschheit. Das klingt nach einer Neuauflage des grünen Alarmismus der 80er-Jahre: Achtung, die Welt geht unter. Doch haben solche Angstszenarien längst an Wirkung eingebüßt.","source":"die tageszeitung, 31.01.2000"},{"example":"Direkt nach der Wende schlugen wir eine größere Untersuchung zu der Entwicklung bei den Skinheads vor - ein hoher Beamter des Jugendministeriums erklärte das alles für Unsinn und unverantwortlichen Alarmismus und hat das verhindert.","source":"Die Zeit, 35/2000"}], [{"example":"Nach der Slow-motion-Bauchlandung von Verteidigungsminister Rühe beim Jäger90 gab der jüngst bekannt, \"notfalls\" auch ohne Grundgesetzänderung Bundeswehrtruppen in UNO-Kriege schicken zu wollen. Auch das paßt besser ins Bild linker Alarmisten als in das ziviler Hoffnungen.","source":"die tageszeitung, 24.11.1992"},{"example":"Randow formulierte damals schon die ersten Regungen einer Wissenschaftsskepsis, die aus der Wissenschaft selber kamen. Lange bevor es Mode wurde, war er ein \"Alarmist\" wie es heute heißt - im Gegensatz zu den \"Beschwichtigern\", die alles halb so schlimm finden, weil sie darauf vertrauen, daß die Wissenschaftler von sich aus dafür sorgen werden, ihre wissenschaftlichen Erkenntnisse für das menschliche Dasein hier auf der Erde vernünftig umzusetzen.","source":"Die Zeit, 18/1995"},{"example":"Immer wieder haben Wissenschaftler die Kritiker der Reproduktionstechnik belehrt und versichert, daß der Klon eine Fiktion ist. Wer das Gegenteil behauptete, galt als Alarmist, der zur Diskreditierung einer ganzen Forschungsrichtung realitätsfremde Schreckensszenarien konstruiert.","source":"die tageszeitung, 01.03.1997"},{"example":"Wenn man vor einem Jahr erklärte: \"Wer Schröder wählt, wählt Krieg\", galt man als Alarmist oder direkt als Verrückter. Dabei gehörte nicht viel Prophezeiungskunst dazu","source":"die tageszeitung, 31.03.1999"},{"example":"Ja, es lässt sich viel gegen die halbstaatlich organisierte Demonstration sagen, die sich am 9. November von der Neuen Synagoge in Berlin zum Brandenburger Tor bewegte. Man kann [...] spotten über die Betroffenheitsformeln der Alarmisten, die nun Weimarer Verhältnisse im Land heraufziehen sehen.","source":"Oberösterreichische Nachrichten, 11.11.2000"}], [{"example":"Lange Zeit war es im Online-Journalismus verpönt, eine gedruckte Zeitung 1:1 ins Internet zu stellen. Doch nachdem die New York Times vor kurzem mit der Ankündigung überrascht hat, im Herbst ein identisches Abbild der gedruckten Ausgabe ins Netz zu stellen, kommt jetzt die Koblenzer Rhein-Zeitung mit dem \"e-paper\" als erste deutsche Zeitung an den Markt.","source":"Frankfurter Allgemeine, 31.05.2001"},{"example":"E-Paper ist das elektronische Abbild der aktuellen Ausgabe der RZ [Rhein-Zeitung] mit allen Texten, Bildern und Inseraten.","source":"Berliner Zeitung, 31.01.2002"},{"example":"25 Zeitungen stehen als E-Paper online. Als erste erschien im Mai 2001 die Rhein-Zeitung. Ihre Auflage liegt bei rund 2 220 Online-Abos. Weitere E-Paper: Rheinische Post (411 Online-Abos), Kieler Nachrichten (231)","source":"Berliner Zeitung, 05.03.2004"},{"example":"Spiegel-Leser können das Nachrichtenmagazin künftig auch online als so genanntes E-Paper kaufen.","source":"Berliner Zeitung, 21.10.2004"},{"example":"Für Deutschlerner weltweit gibt es das Heft zum gleichen Preis (5,50 Euro) auch als E-Paper. 2 000 Abonnenten konnte der Verlag schon gewinnen - in 31 Ländern.","source":"Berliner Zeitung, 20.10.2005"},{"example":"Den Spiegel als Magazin kaufe ich mir nicht mehr, zehn Minuten als E-Paper, ein bisschen FAZ [...], das war's dann.","source":"Die Zeit, 23.11.2006, Nr. 48"},{"example":"Sie können das aktuelle e-paper der F.A.Z. nur mit gültigem e-paper Abonnement und nach erfolgter Anmeldung vollständig nutzen.","source":"www.faz.net; recherchiert am 05.12.2006"},{"example":"Gemeinsam sahen sich die Floriansjünger dann auf der Homepage der Feuerwehr Altenkirchen und im E-Paper der Rhein-Zeitung die neuesten Bilder aus der Heimat an.","source":"Rhein-Zeitung, 05.03.2007"},{"example":"Etwas unbefriedigend sind auf jeden Fall das grobmaschige Stichwortverzeichnis und das Fehlen der Übersicht bei den E-Papers.","source":"St. Galler Tagblatt, 27.05.2008"},{"example":"Für Nichtabonnenten dagegen wird das E-Paper ab 17. August kostenpflichtig (249 Franken pro Jahr) - die Printausgabe kostet derzeit 339 Franken pro Jahr.","source":"St. Galler Tagblatt, 13.07.2009"},{"example":"Mathias Müller von Blumencron (49), Chefredakteur des Spiegel, hat einen Pakt mit dem Teuf... äh ... der Telekom geschlossen. Auf der Cebit kündigte Blumencron an, dass E-Paper und App des Magazins künftig per Telefonrechnung bezahlt werden können.","source":"die tageszeitung, 05.03.2010"},{"example":"Anfang der Woche hat sie eine E-Paper ins Internet gestellt","source":"Berliner Zeitung, 05.03.2004"}], [{"example":"Gut ein Dutzend Firmen rund um den Globus arbeiten intensiv daran, \"elektronisches Papier\" (\" E-Paper\") auf den Markt zu bringen. Es soll sich wie normales Papier anfühlen und auch so aussehen, in Wirklichkeit aber ein wiederbeschreibbarer Computerbildschirm sein. Das digitale Papier gehört zu den Zukunftstechnologien, die den Alltag der Menschen grundlegend ändern könnten.","source":"Vorarlberger Nachrichten, 31.12.1999"},{"example":"Auch für den Buchmarkt könnte das E-Paper von Bedeutung sein. Nachdem sich das viel beschworene E-Book als Ladenhüter entpuppt hat, sollen raumgreifende Bücherregale künftig durch das E-Paper überflüssig werden. Die Fibeln der elektronischen Generation werden zwar weiterhin einige hundert weiße und flexible Seiten aufweisen, auf denen man Anmerkungen schreiben oder ganze Textpassagen unterstreichen kann. Doch reicht es vollkommen aus, lediglich ein einziges Buch zu besitzen. Schlägt man es auf, erscheint eine Liste mit allen gespeicherten Werken. Nach Autorennamen, Titeln oder Sachgebieten geordnet, lässt sich schnell der gewünschte Text auswählen und auf die vorhandenen Buchseiten laden. Die Vorteile gegenüber einem E-Book liegen auf der Hand: Wie bei konventionellen Büchern lässt sich einfach vor- und zurückblättern, und man kann sich auf den Seiten gut räumlich orientieren. Außerdem ist das E-Paper leicht, faltbar und reagiert auf Druck.","source":"Berliner Zeitung, 14.03.2001"},{"example":"E-Paper statt Papier [Überschrift] Alle Zeitungen auf einen Blick, dazu Goethes gesammelte Werke und Internetanschluss - das alles soll ein neues Gerät, das E-Paper, ermöglichen. So vielseitig wie ein Computer, so praktisch wie Papier. Denn das E-Paper ist nicht größer als ein A4-Blatt und man kann es knicken.","source":"Berliner Zeitung, 14.03.2001"},{"example":"Der eigentliche Vorteil des E-Papers gegenüber dem herkömmlichen Papier wird sich aber vor allem dann zeigen, wenn die Schrift auf den Tafeln in Sekundenschnelle und beliebig oft verändert werden kann. Dazu müssten die Daten allerdings über Draht oder Mobilfunk aus dem Computer auf das Papier übertragen werden.","source":"Die Zeit [Online-Ausgabe], 07.02.2002, Nr. 07"},{"example":"Fixes e-Paper / Elektrisch gesteuerte Benetzung sorgt für bewegte Bilder [Überschriften] Die Industrie bastelt fleißig an e-Paper-Konzepten, um uns in Zukunft die Zeitung in elektronischer Form direkt aufs Papier zu zaubern - bei Bedarf auch mit bewegten Bildern.","source":"spektrumdirekt, 24.09.2003"},{"example":"Dieses Jahr, 2006 verspricht die erste harte Testphase für das elektronische Papier, neudeutsch E-Paper, zu werden.","source":"VDI nachrichten, 05.05.2006"},{"example":"Die erste Fabrik für so genanntes E-Paper - dünne, biegsame Bildschirme, die mit der Zeit das Erscheinungsbild von Zeitungen oder den Umgang mit Dokumenten revolutionieren könnten - wird in Dresden gebaut.","source":"Mannheimer Morgen, 04.01.2007"},{"example":"Beim E-Paper reagieren Farbstoffpartikel auf einer Kunststoffschicht auf elektrische Spannung.","source":"Mannheimer Morgen, 04.01.2007"},{"example":"Ein anderes Prinzip ist das von Fuji-Xerox entwickelte flexible E-Paper aus cholesterischen Flüssigkristallen.","source":"VDI nachrichten, 04.07.2008"},{"example":"Knicken können Sie das E-Paper von LG Display zwar nicht. Dafür soll es sich aber gut rollen lassen – und selbständig zurück in seine ursprüngliche, flache Form zurückkehren.","source":"http://digitalleben.t-online.de; datiert vom 20.01.2010"},{"example":"Das E-Paper besteht aus einer nur 0,2 Millimeter dicken Kunststoff-Folie, in die Millionen kleiner Kapseln eingelagert sind. Diese Kapseln enthalten jeweils einen frei drehbaren schwarz-weißen \"Kern\", der in einem elektrischen Feld als Dipol reagiert. Die Buchstaben und Zeichen entstehen dadurch, dass die \"Kerne\" ihre schwarze oder weiße Seite präsentieren. Um dies zu erreichen, wird die Druckvorlage in ein elektrisches Spannungsmuster übersetzt. Analog zu einer Farbrolle beim herkömmlichen Druck überträgt das Spannungsmuster die Information Stück für Stück aufs E-Paper. Das so erzeugte Bild oder der so erzeugte Text bleiben bestehen, bis ein neues elektrisches Feld andere Zeichen setzt.","source":"Berliner Morgenpost, 27.08.1999"},{"example":"Die Hersteller von elektronischem Papier sind derzeit bemüht, das Trägermedium aus Plastik immer dünner zu machen, möglichst so dünn und flexibel wie herkömmliches Papier.","source":"dpa, 12.03.2009"}], [{"example":"\"Machen wir den Oskar, oder stärken wir Joschka den Rücken?\" Vor dieser Entscheidung standen die Main-Kinzig-Grünen am Mittwoch abend","source":"Frankfurter Rundschau, 07.05.1999"},{"example":"Ein Satz aus der letzten «Tatort»-Episode hatte Spekulationen aufkommen lassen, Krug wolle möglicherweise die Krimi-Serie verlassen. Dort hiess es:« Noch zwei Jahre, dann machen wir den Lafontaine.»","source":"St. Galler Tagblatt, 19.06.1999"},{"example":"Ich bin gewählter Bundestagsabgeordneter, und ich finde nicht, dass man dann einfach sagen kann: Ich mach jetzt den Oskar.","source":"die tageszeitung, 06.04.2000"},{"example":"[A:] Einen männlichen Politiker würden Sie übrigens vermutlich nicht nach dem Gefühl fragen - da gilt Machtanspruch als Zeichen von Politikfähigkeit. Bei Frauen wird es als Provokation empfunden. [B:] Machen Sie jetzt hier den Westerwelle? [A:] Nein. Zwischen uns gibt es einen großen Unterschied. Westerwelle will einfach nur an die Macht. Wir wollen Macht haben, um verändern zu können.","source":"die tageszeitung, 16.03.2002"},{"example":"Seit der 54-jährige PDS-Star [Gregor Gysi] \"den Lafontaine gemacht\" und sich unvermittelt ins Privatleben zurückgezogen hat, sind die Sympathien für ihn deutlich gesunken","source":"Nürnberger Nachrichten, 02.08.2002"},{"example":"Juror Ralph Siegel machte den Bohlen und erzählte Kandidatinnen, dass sie zu dick oder zu alt seien.","source":"die tageszeitung, 01.09.2003"},{"example":"Als ob die zermürbenden innerparteilichen Ränkespiele zur Gesundheitsreform und neuerdings auch zum Türkei-Problem nicht längst das Fass zum Überlaufen bringen, macht nun auch noch Friedrich Merz den Lafontaine. Genauso wie der Saarländer vor fünf Jahren schmeißt jetzt der Sauerländer ohne offizielle Begründung seine Ämter in Partei und Fraktion hin.","source":"Rhein-Zeitung, 13.10.2004"},{"example":"\"Wir wollen nicht den Dieter Hallervorden machen und 30 Witze hintereinander wegspielen\", betont Bischof.","source":"Mannheimer Morgen, 22.04.2005"},{"example":"Aber vielleicht kommt alles anders. Nämlich wenn die Merkel tatsächlich den Stalin macht. Und genüsslich schweigend einen Gegner nach dem anderen abräumt","source":"die tageszeitung, 11.10.2005"},{"example":"Im Moment, als Müntefering den Schröder machte, waren sich da nicht alle so sicher. Nahles hatte Münteferings drohende Andeutungen damals nicht richtig gedeutet.","source":"Berliner Zeitung, 31.10.2006"},{"example":"In einer Situation, in der er [Kurt Beck] als Parteichef Stärke zeigen und Verantwortung übernehmen müsste, macht er lieber den Stoiber und bleibt in der Provinz.","source":"dpa, 14.11.2007"},{"example":"[wie kann man auch wissen,] dass die Iren gleich die Griechen machen [im Zusammenhang mit der Beantragung des Rettungsschirmes]","source":"Fernsehen [ZDF], 12.11.2010"},{"example":"Tatsächlich hat selten jemand den Bettel so effektvoll hingeschmissen wie Oskar Lafontaine, SPD-Vorsitzender und Finanzminister in der Regierung Schröder, am 11. März dieses Jahres: eine knappe Notiz ans Kanzleramt, keine erklärenden Worte ausser schmalen Äusserungen über das \"schlechte Mannschaftsspiel\" in der Regierung.","source":"Tages-Anzeiger, 27.09.1999"},{"example":"Thomas Anders macht den Bohlen [...] Sänger Thomas Anders macht seinen Ex-\"Modern-Talking\"-Partner Dieter Bohlen nach","source":"Hamburger Morgenpost, 10.10.2009"}]
    /// </summary>
    [JsonProperty("cit")]
    public IList<Cit> Cit { get; set; }

    /// <summary>
    /// Examples: [{"type":"synonym","target":"https://www.owid.de/artikel/316400","value":"elektronisches Papier"}], [{"type":"related","target":"https://www.owid.de/artikel/401240","value":"facebooken"},{"type":"related","target":"https://www.owid.de/artikel/317335","value":"twittern"},{"type":"related","target":"https://www.owid.de/artikel/317668","value":"zwitschern"},{"type":"related","target":"https://www.owid.de/artikel/407497","value":"youtuben"},{"type":"related","target":"https://www.owid.de/artikel/408478","value":"snapchatten"},{"type":"related","target":"https://www.owid.de/artikel/408478","value":"instagrammisieren"}], [{"type":"related","target":"https://www.owid.de/artikel/401240","value":"facebooken"},{"type":"related","target":"https://www.owid.de/artikel/317335","value":"twittern"},{"type":"related","target":"https://www.owid.de/artikel/317668","value":"zwitschern"},{"type":"related","target":"https://www.owid.de/artikel/407497","value":"youtuben"},{"type":"related","target":"https://www.owid.de/artikel/408478","value":"snapchatten"},{"type":"related","target":"https://www.owid.de/artikel/408478","value":"instagrammisieren"}], [{"type":"related","target":"https://www.owid.de/artikel/401622","value":"Der Weg zur Hölle ist mit guten Vorsätzen gepflastert"}], [{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Mietradsystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Mietrad-System"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Miet-Rad-System"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Miet-Radsystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Mieträdersystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Miet-Rädersystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Mieträder-System"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Miet-Räder-System"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Mietradelsystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Miet-Radel-System"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Mietradel-System"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Miet-Radelsystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Leihradsystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Leihrad-System"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Leih-Radsystem"},{"type":"synonym","target":"https://www.owid.de/artikel/408445","value":"Leih-Rad-System"}]
    /// </summary>
    [JsonProperty("xr")]
    public IList<Xr> Xr { get; set; }
  }

}