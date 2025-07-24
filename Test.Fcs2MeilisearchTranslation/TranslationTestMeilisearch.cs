using IDS.TextPlus.FCSEndpoint.Parser;
using IDS.TextPlus.FCSEndpoint.Traslator;

namespace Test.Fcs2MeilisearchTranslation
{
  public class TranslationTestMeilisearch
  {
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    // Eigene Tests (JOR)
    [TestCase("fu", "fu", null)]
    [TestCase("pandemie OR corona", "*", "lemma_token = \"pandemie\" OR lemma_token = \"corona\"")]
    //[TestCase("pandemie AND NOT corona", "*", "lemma_token = \"pandemie\" AND lemma_token = NOT corona")]
    [TestCase("fu AND source = neo", "fu", "source = neo")]
    [TestCase("source = neo", "*", "source = neo")]
    [TestCase("source == neo", "*", "source = neo")]
    [TestCase("fu AND source != neo", "fu", "source != neo")]
    [TestCase("fu AND source NOT neo", "fu", "source NOT neo")] // in FCS nicht zulässig (NOT -> !=)
    [TestCase("(pandemie AND corona)", "*", "lemma_token = \"pandemie\" AND lemma_token = \"corona\"")]
    [TestCase("Auge AND source = neo OR source = sprw", "Auge", "source = neo OR sprw")]
    [TestCase("lemma = corona AND text = pandemie", "*", "lemma = corona AND text = pandemie")]
    [TestCase("pos=NOUN AND gender=Masc", "*", "gender = Masc AND pos = NOUN")]
    // Tests von: https://clarin-eric.github.io/fcs-misc/fcs-core-2.0-specs/fcs-core-2.0.html#_basic_search
    [TestCase("cat", "cat", null)]
    [TestCase("\"cat\"", "\"cat\"", null)]
    [TestCase("cat AND dog", "*", "lemma_token = \"cat\" AND lemma_token = \"dog\"")]
    [TestCase("\"grumpy cat\"", "\"grumpy cat\"", null)]
    [TestCase("\"grumpy cat\" AND dog", "*", "lemma_token = \"grumpy cat\" AND lemma_token = dog")] // TODO: Der Parser übersieht die OR-Bedingung
    [TestCase("\"grumpy cat\" OR \"lazy dog\"", "*", "lemma_token = \"grumpy cat\" OR lemma_token = \"lazy dog\"")] // TODO: Der Parser übersieht die OR-Bedingung
    [TestCase("cat AND (mouse OR \"lazy dog\")", "cat AND (mouse OR \"lazy dog\")", null)] // TODO: Klammerung > Priorität
    // Tests von: https://clarin-eric.github.io/fcs-misc/fcs-core-2.0-specs/fcs-core-2.0.html#_fcs_ql
    [TestCase("\"walking\"", "\"walking\"", null)]
    [TestCase("[token = \"walking\"]", "token = \"walking\"", null)]
    // [TestCase("\"Dog\" /c", "\"Dog\" /c", null)] // TODO: /c - nachschlagen&implementieren
    //[TestCase("[word = \"Dog\" /c]", "word = \"Dog\" /c", null)] // TODO: /c - nachschlagen&implementieren
    [TestCase("[pos = \"NOUN\"]", "*", "pos = \"NOUN\"")]
    [TestCase("[pos != \"NOUN\"]", "*", "pos != \"NOUN\"")]
    [TestCase("[lemma = \"walk\"]", "*", "lemma = \"walk\"")]
    [TestCase("\"blaue|grüne\" [pos = \"NOUN\"]", "\"blaue|grüne\"", "pos = \"NOUN\"")]
    //[TestCase("\"dogs\" []{3,} \"cats\" within s", "\"dogs\" []{3,} \"cats\" within s", null)] // TODO: z:pos - mEn für OWID nicht relevant - überprüfen
    //[TestCase("[z:pos = \"ADJ\"]", "*", "z:pos = \"ADJ\"")] // TODO: z:pos - mEn für OWID nicht relevant - überprüfen
    //[TestCase("[z:pos = \"ADJ\" & q:pos = \"ADJ\"]", "*", "z:pos = \"ADJ\" & q:pos = \"ADJ\"")] // TODO: z:pos - mEn für OWID nicht relevant - überprüfen
    public void ValidateExpression(string query, string outputQuery, string? outputFilter)
    {
      var translate = TranslateFcs2Meilisearch.Create(FCSQuery.Parse(query).main_query());
      Assert.That(translate.Query, Is.EqualTo(outputQuery));
      Assert.That(translate.Filter, Is.EqualTo(outputFilter));
      Assert.Pass();
    }
  }
}