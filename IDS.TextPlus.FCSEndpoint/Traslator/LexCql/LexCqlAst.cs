namespace IDS.TextPlus.FCSEndpoint.Traslator.LexCql;

/// <summary>
///   Modifier that can follow a relation operator (e.g. =/ignoreCase).
/// </summary>
public enum RelationModifier
{
  None,
  IgnoreCase,
  RespectCase,
  Regex
}

/// <summary>
///   The relation operator between field and value.
/// </summary>
public enum RelationOp
{
  /// <summary>= (full-text / match search)</summary>
  Equals,

  /// <summary>== (exact / term match)</summary>
  ExactEquals,

  /// <summary>!= (field must not equal value)</summary>
  NotEquals
}

// ── AST node hierarchy ──────────────────────────────────────────────

/// <summary>
///   Base class for all LexCQL expression nodes.
/// </summary>
public abstract record LexCqlExpression;

/// <summary>
///   A field-relation-value comparison, e.g. <c>lemma=/ignoreCase "cat"</c>.
/// </summary>
/// <param name="Field">The field name (e.g. "lemma", "pos"). Null only when implicit default is used.</param>
/// <param name="Relation">The relation operator.</param>
/// <param name="Modifier">Optional modifier on the relation.</param>
/// <param name="Value">The search value (unescaped).</param>
public sealed record ComparisonExpression(
  string Field,
  RelationOp Relation,
  RelationModifier Modifier,
  string Value
) : LexCqlExpression;

/// <summary>
///   Logical AND of two expressions.
/// </summary>
public sealed record AndExpression(LexCqlExpression Left, LexCqlExpression Right) : LexCqlExpression;

/// <summary>
///   Logical OR of two expressions.
/// </summary>
public sealed record OrExpression(LexCqlExpression Left, LexCqlExpression Right) : LexCqlExpression;

/// <summary>
///   Logical NOT (negation) of an expression.
/// </summary>
public sealed record NotExpression(LexCqlExpression Inner) : LexCqlExpression;