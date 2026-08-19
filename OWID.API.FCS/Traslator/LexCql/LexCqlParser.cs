using System.Text;
using Superpower;
using Superpower.Parsers;

namespace IDS.TextPlus.FCSEndpoint.Traslator.LexCql;

/// <summary>
///   Superpower token-list parser for LexCQL.
///   Produces a <see cref="LexCqlExpression" /> AST from a token stream.
/// </summary>
public static class LexCqlParser
{
  private const string DefaultField = "lemma";

  // ── Leaf parsers ───────────────────────────────────────────────────

  /// <summary>
  ///   Parses AND / OR / NOT identifiers as their dedicated token kinds.
  ///   The tokenizer emits them as Identifier; the parser distinguishes them here.
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, string> And =
    Token.EqualTo(LexCqlToken.Identifier)
      .Where(t => t.ToStringValue() == "AND", "AND")
      .Value("AND");

  private static readonly TokenListParser<LexCqlToken, string> Or =
    Token.EqualTo(LexCqlToken.Identifier)
      .Where(t => t.ToStringValue() == "OR", "OR")
      .Value("OR");

  private static readonly TokenListParser<LexCqlToken, string> Not =
    Token.EqualTo(LexCqlToken.Identifier)
      .Where(t => t.ToStringValue() == "NOT", "NOT")
      .Value("NOT");

  /// <summary>
  ///   Parses the relation operator: =, ==, or !=
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, RelationOp> RelationOp =
    Token.EqualTo(LexCqlToken.NotEquals).Value(LexCql.RelationOp.NotEquals)
      .Or(Token.EqualTo(LexCqlToken.DoubleEquals).Value(LexCql.RelationOp.ExactEquals))
      .Or(Token.EqualTo(LexCqlToken.Is).Value(LexCql.RelationOp.IsMapping))
      .Or(Token.EqualTo(LexCqlToken.Equals).Value(LexCql.RelationOp.Equals));

  /// <summary>
  ///   Parses an optional modifier: /ignoreCase, /respectCase, /regexp
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, RelationModifier> Modifier =
    (from slash in Token.EqualTo(LexCqlToken.Slash)
      from name in Token.EqualTo(LexCqlToken.Identifier)
      select ParseModifier(name.ToStringValue()))
    .OptionalOrDefault();

  /// <summary>
  ///   Parses a value — either a quoted string or a bare identifier/word.
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, string> Value =
    Token.EqualTo(LexCqlToken.QuotedString).Select(t => UnescapeQuoted(t.ToStringValue()))
      .Or(Token.EqualTo(LexCqlToken.Identifier)
        .Where(t => !IsKeyword(t.ToStringValue()), "value")
        .Select(t => UnescapeBare(t.ToStringValue())));

  /// <summary>
  ///   Parses a field name (any non-keyword identifier).
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, string> FieldName =
    Token.EqualTo(LexCqlToken.Identifier)
      .Where(t => !IsKeyword(t.ToStringValue()), "field name")
      .Select(t => t.ToStringValue());

  // ── Expression parsers ─────────────────────────────────────────────

  /// <summary>
  ///   Explicit comparison: field relation value  (e.g. lemma = "cat")
  ///   Includes optional modifier after relation.
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> ExplicitComparison =
    from field in FieldName
    from relation in RelationOp
    from modifier in Modifier
    from value in Value
    select (LexCqlExpression)new ComparisonExpression(field, relation, modifier, value);

  /// <summary>
  ///   Implicit comparison: a bare value mapped to <c>lemma = value</c>.
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> ImplicitComparison =
    Value.Select(v =>
      (LexCqlExpression)new ComparisonExpression(DefaultField, LexCql.RelationOp.Equals, RelationModifier.None, v));

  /// <summary>
  ///   NOT factor: <c>NOT expr</c>
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> NotFactor =
    from _ in Not
    from inner in Parse.Ref(() => Factor)
    select (LexCqlExpression)new NotExpression(inner);

  /// <summary>
  ///   Parenthesised expression.
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> Parenthesised =
    from lp in Token.EqualTo(LexCqlToken.LParen)
    from expr in Parse.Ref(() => Expr)
    from rp in Token.EqualTo(LexCqlToken.RParen)
    select expr;

  /// <summary>
  ///   A single factor: parenthesised, NOT-prefixed, explicit comparison, or implicit comparison (in that order).
  ///   Explicit must be tried before implicit so that a field name isn't consumed as a bare value.
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> Factor =
    Parenthesised
      .Or(NotFactor)
      .Or(ExplicitComparison.Try())
      .Or(ImplicitComparison);

  /// <summary>
  ///   AND chain (higher precedence than OR).
  ///   Supports both explicit AND and implicit AND before NOT (e.g. <c>pos = NOUN NOT definition = Partner</c>).
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> AndChain =
    from first in Factor
    from rest in
      And.IgnoreThen(Factor) // explicit AND
        .Or(NotFactor) // implicit AND before NOT
        .Many()
    select rest.Aggregate(first, (left, right) => new AndExpression(left, right));

  /// <summary>
  ///   OR chain (lowest precedence).
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> OrChain =
    Parse.Chain(
      Or.Value((Func<LexCqlExpression, LexCqlExpression, LexCqlExpression>)((l, r) => new OrExpression(l, r))),
      AndChain,
      (op, left, right) => op(left, right));

  /// <summary>
  ///   Top-level expression.
  /// </summary>
  private static readonly TokenListParser<LexCqlToken, LexCqlExpression> Expr = OrChain;

  /// <summary>
  ///   Complete query: expression consuming all tokens.
  /// </summary>
  public static readonly TokenListParser<LexCqlToken, LexCqlExpression> Query = Expr.AtEnd();

  // ── Helpers ────────────────────────────────────────────────────────

  /// <summary>
  ///   Checks whether an identifier token text is a keyword (AND/OR/NOT).
  /// </summary>
  private static bool IsKeyword(string text)
  {
    return text is "AND" or "OR" or "NOT";
  }

  // ── String helpers ─────────────────────────────────────────────────

  private static RelationModifier ParseModifier(string name)
  {
    return name switch
    {
      "ignoreCase" => RelationModifier.IgnoreCase,
      "respectCase" => RelationModifier.RespectCase,
      "regexp" => RelationModifier.Regex,
      _ => throw new LexCqlParseException($"Unknown modifier: '{name}'. Allowed: ignoreCase, respectCase, regexp.")
    };
  }

  /// <summary>
  ///   Removes surrounding quotes and processes escape sequences in a quoted string.
  ///   Supports: \" \\ \* \?
  /// </summary>
  private static string UnescapeQuoted(string raw)
  {
    // Remove surrounding quotes
    var inner = raw[1..^1];
    return Unescape(inner);
  }

  /// <summary>
  ///   Processes escape sequences in bare (unquoted) identifiers.
  /// </summary>
  private static string UnescapeBare(string raw)
  {
    return Unescape(raw);
  }

  private static string Unescape(string input)
  {
    if (!input.Contains('\\'))
      return input;

    var sb = new StringBuilder(input.Length);
    for (var i = 0; i < input.Length; i++)
      if (input[i] == '\\' && i + 1 < input.Length && input[i + 1] is '"' or '\\' or '*' or '?')
      {
        sb.Append(input[i + 1]);
        i++; // skip next character
      }
      else
      {
        sb.Append(input[i]);
      }

    return sb.ToString();
  }
}

/// <summary>
///   Exception thrown when parsing a LexCQL query fails.
/// </summary>
public class LexCqlParseException : Exception
{
  public LexCqlParseException(string message) : base(message)
  {
  }

  public LexCqlParseException(string message, Exception innerException) : base(message, innerException)
  {
  }
}