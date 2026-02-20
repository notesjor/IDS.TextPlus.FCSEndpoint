using System.Text.Json;
using System.Text.Json.Nodes;
using IDS.TextPlus.FCSEndpoint.Traslator.LexCql;
using Superpower;

namespace IDS.TextPlus.FCSEndpoint.Traslator;

/// <summary>
/// Translates a LexCQL query string into an Elasticsearch query.
/// Uses Superpower for tokenization and parsing, producing a clean AST
/// that is then converted into Elasticsearch JSON.
/// </summary>
public class TranslatorFcs2ElasticsearchV2
{
  private TranslatorFcs2ElasticsearchV2() { }

  /// <summary>
  /// The parsed AST of the LexCQL query.
  /// </summary>
  public LexCqlExpression Expression { get; private set; } = null!;

  /// <summary>
  /// The Elasticsearch query as a <see cref="JsonObject"/>.
  /// </summary>
  public JsonObject ElasticsearchQuery => ElasticsearchQueryBuilder.Build(Expression);

  /// <summary>
  /// The Elasticsearch query serialized as a JSON string (compact).
  /// </summary>
  public string ElasticsearchQueryJson => ElasticsearchQueryBuilder.BuildJson(Expression);

  /// <summary>
  /// The Elasticsearch query serialized as a pretty-printed JSON string.
  /// </summary>
  public string ElasticsearchQueryJsonIndented => ElasticsearchQueryBuilder.BuildJson(Expression, indented: true);

  /// <summary>
  /// Parses a LexCQL query string and returns a translator instance
  /// containing the AST and the corresponding Elasticsearch query.
  /// </summary>
  /// <param name="query">The raw LexCQL query string.</param>
  /// <returns>A translator instance with the parsed result.</returns>
  /// <exception cref="LexCqlParseException">Thrown when the query cannot be parsed.</exception>
  public static TranslatorFcs2ElasticsearchV2 Parse(string query)
  {
    if (string.IsNullOrWhiteSpace(query))
      throw new LexCqlParseException("Query must not be empty.");

    try
    {
      var tokens = LexCqlTokenizer.Instance.Tokenize(query.Trim());
      var ast = LexCqlParser.Query.Parse(tokens);

      return new TranslatorFcs2ElasticsearchV2 { Expression = ast };
    }
    catch (LexCqlParseException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new LexCqlParseException($"Failed to parse LexCQL query: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Attempts to parse a LexCQL query string. Returns false on failure instead of throwing.
  /// </summary>
  /// <param name="query">The raw LexCQL query string.</param>
  /// <param name="result">The translator instance if parsing succeeds; null otherwise.</param>
  /// <param name="error">The error message if parsing fails; null otherwise.</param>
  /// <returns>True if parsing succeeded; false otherwise.</returns>
  public static bool TryParse(string query, out TranslatorFcs2ElasticsearchV2? result, out string? error)
  {
    try
    {
      result = Parse(query);
      error = null;
      return true;
    }
    catch (Exception ex)
    {
      result = null;
      error = ex.Message;
      return false;
    }
  }
}
