using System.Text.Json;
using System.Text.Json.Nodes;

namespace IDS.TextPlus.FCSEndpoint.Traslator.LexCql;

public static class ElasticsearchQueryBuilder
{
  public static JsonObject Build(LexCqlExpression expression)
  {
    ArgumentNullException.ThrowIfNull(expression);
    return BuildNode(expression);
  }

  public static string BuildJson(LexCqlExpression expression, bool indented = false)
  {
    var query = new JsonObject { ["query"] = Build(expression) };
    var options = new JsonSerializerOptions { WriteIndented = indented };
    return query.ToJsonString(options);
  }

  private static JsonObject BuildNode(LexCqlExpression expression)
  {
    return expression switch
    {
      ComparisonExpression cmp => BuildComparison(cmp),
      AndExpression and => BuildBool("must", BuildNode(and.Left), BuildNode(and.Right)),
      OrExpression or => BuildBool("should", BuildNode(or.Left), BuildNode(or.Right)),
      NotExpression not => BuildMustNot(BuildNode(not.Inner)),
      _ => throw new InvalidOperationException($"Unknown expression type: {expression.GetType().Name}")
    };
  }

  private static JsonObject BuildComparison(ComparisonExpression cmp)
  {
    var field = cmp.Field;
    var value = cmp.Value;
    var isExact = cmp.Relation == RelationOp.ExactEquals;

    var clause = cmp.Modifier switch
    {
      RelationModifier.Regex => BuildRegexpClause(field, value),
      _ when ContainsWildcard(value) => BuildWildcardClause(field, value, cmp.Modifier == RelationModifier.IgnoreCase),
      RelationModifier.IgnoreCase => BuildMatchClause(field, value),
      RelationModifier.RespectCase => BuildTermClause(field, value),
      _ when isExact => BuildTermClause(field, value),
      RelationModifier.None => BuildDefaultClause(field, value),
      _ => BuildDefaultClause(field, value)
    };

    return cmp.Relation == RelationOp.NotEquals
      ? BuildMustNot(clause)
      : clause;
  }

  private static bool ContainsWildcard(string value)
  {
    for (var i = 0; i < value.Length; i++)
      if (value[i] is '*' or '?')
        return true;
    return false;
  }

  // ── Elasticsearch clause builders ──────────────────────────────────

  /// <summary>
  ///   Default clause
  /// </summary>
  private static JsonObject BuildDefaultClause(string field, string value)
  {
    return BuildMatchClause(field, value);
  }

  /// <summary>
  ///   Match clause: full-text search (case-insensitive by default through analyzers).
  ///   Used for /ignoreCase modifier.
  /// </summary>
  private static JsonObject BuildMatchClause(string field, string value)
  {
    return new JsonObject
    {
      ["match"] = new JsonObject
      {
        [field] = new JsonObject
        {
          ["query"] = value
        }
      }
    };
  }

  /// <summary>
  ///   Term clause: exact match (case-sensitive on keyword fields).
  ///   Used for /respectCase modifier.
  /// </summary>
  private static JsonObject BuildTermClause(string field, string value)
  {
    return new JsonObject
    {
      ["term"] = new JsonObject
      {
        [field] = new JsonObject
        {
          ["value"] = value
        }
      }
    };
  }

  /// <summary>
  ///   Wildcard clause for patterns with * and ?.
  ///   Targets the field directly — works on keyword fields.
  ///   If a field has a dedicated wildcard sub-field (e.g. lemma.query), ES will use the main keyword field.
  /// </summary>
  private static JsonObject BuildWildcardClause(string field, string value, bool caseInsensitive)
  {
    var fieldObj = new JsonObject { ["value"] = value };

    if (caseInsensitive)
      fieldObj["case_insensitive"] = true;

    return new JsonObject
    {
      ["wildcard"] = new JsonObject { [field] = fieldObj }
    };
  }

  /// <summary>
  ///   Regexp clause for /regex modifier.
  /// </summary>
  private static JsonObject BuildRegexpClause(string field, string value)
  {
    return new JsonObject
    {
      ["regexp"] = new JsonObject
      {
        [field] = new JsonObject
        {
          ["value"] = value
        }
      }
    };
  }

  // ── Boolean wrappers ───────────────────────────────────────────────

  private static JsonObject BuildBool(string occur, params JsonObject[] clauses)
  {
    var array = new JsonArray();
    foreach (var clause in clauses)
      array.Add(clause);

    var boolInner = new JsonObject { [occur] = array };

    if (occur == "should")
      boolInner["minimum_should_match"] = 1;

    return new JsonObject { ["bool"] = boolInner };
  }

  private static JsonObject BuildMustNot(JsonObject clause)
  {
    return new JsonObject
    {
      ["bool"] = new JsonObject
      {
        ["must_not"] = new JsonArray { clause }
      }
    };
  }
}