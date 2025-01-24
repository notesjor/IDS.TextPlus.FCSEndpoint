using IDS.TextPlus.FCSEndpoint.Parser;
using IDS.TextPlus.FCSEndpoint.Traslator;

namespace Test.Fcs2MeilisearchTranslation
{
  public class Tests
  {
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase("fu", "fu", null)]
    [TestCase("pandemie OR corona", "pandemie OR corona", null)]
    [TestCase("fu AND source = neo", "fu", "source = neo")]
    [TestCase("(pandemie AND corona)", "pandemie AND corona", null)]
    [TestCase("Auge AND source = neo OR source = sprw", "Auge", "source = neo OR sprw")]
    [TestCase("lemma = corona AND text = pandemie", "*", "text = pandemie AND lemma = corona")]
    public void ValidateExpression(string query, string outputQuery, string? outputFilter)
    {
      var translate = TranslateFcs2Meilisearch.Create(FCSQuery.Parse(query).main_query());
      Assert.That(translate.Query, Is.EqualTo(outputQuery));
      Assert.That(translate.Filter, Is.EqualTo(outputFilter));
      Assert.Pass();
    }
  }
}