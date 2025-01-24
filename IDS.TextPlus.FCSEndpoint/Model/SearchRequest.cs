using IDS.TextPlus.FCSEndpoint.Parser;
using IDS.TextPlus.FCSEndpoint.Traslator;

namespace IDS.TextPlus.FCSEndpoint.Model;

/// <summary>
/// This class is a representation of a meilisearch request (https://www.meilisearch.com/docs/reference/api/search).
/// 
/// </summary>
public class SearchRequest
{
  public string q { get; set; }
  public int limit { get; set; } = 10;
  public int offset { get; set; } = 0;
  public string filter { get; set; }

  public string[] attributesToHighlight { get; set; } = { "lemma", "text" };
  public string highlightPreTag { get; set; } = "<hits:Hit>";
  public string highlightPostTag { get; set; } = "</hits:Hit>";

  /// <summary>
  /// Converts a FCS-Query to a MeiliSearch query.
  /// </summary>
  /// <param name="query">FCS-query</param>
  /// <exception cref="TypeLoadException">This exception is thrown if something is wrong with this query</exception>
  public void SetQuery(string query)
  {
    var parts = FCSQuery.Parse(query);
    if (parts.NumberOfSyntaxErrors > 0)
      throw new TypeLoadException("Invalid query");

    var translator = TranslateFcs2Meilisearch.Create(parts.main_query());
    q = translator.Query;
    filter = translator.Filter;
  }
}