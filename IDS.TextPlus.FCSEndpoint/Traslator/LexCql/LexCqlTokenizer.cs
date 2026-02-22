using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace IDS.TextPlus.FCSEndpoint.Traslator.LexCql;

/// <summary>
///   Tokenizer for LexCQL queries. Splits raw input into a stream of <see cref="LexCqlToken" /> tokens.
/// </summary>
public static class LexCqlTokenizer
{
  /// <summary>
  ///   Recognizer for a quoted string literal with backslash escaping (\", \\, \*, \?).
  /// </summary>
  private static readonly TextParser<Unit> QuotedStringToken =
    from open in Character.EqualTo('"')
    from content in Character.EqualTo('\\').IgnoreThen(Character.AnyChar).Value(Unit.Value)
      .Or(Character.Except('"').Value(Unit.Value))
      .Many()
    from close in Character.EqualTo('"')
    select Unit.Value;

  /// <summary>
  ///   Recognizer for an unquoted identifier (letters, digits, underscores).
  ///   Includes wildcard characters * and ? so bare terms like "Berg*" are captured as a single token.
  /// </summary>
  private static readonly TextParser<Unit> IdentifierToken =
    Character.LetterOrDigit
      .Or(Character.EqualTo('_'))
      .Or(Character.EqualTo('*'))
      .Or(Character.EqualTo('?'))
      .Or(Character.EqualTo('\\').IgnoreThen(Character.AnyChar)) // escaped chars in bare words
      .Or(Character.EqualTo('\'')) // apostrophe for words like "car's"
      .AtLeastOnce()
      .Value(Unit.Value);

  /// <summary>
  ///   Pre-built tokenizer instance. Thread-safe and reusable.
  /// </summary>
  public static readonly Tokenizer<LexCqlToken> Instance = new TokenizerBuilder<LexCqlToken>()
    .Ignore(Span.WhiteSpace)
    .Match(Character.EqualTo('('), LexCqlToken.LParen)
    .Match(Character.EqualTo(')'), LexCqlToken.RParen)
    .Match(Character.EqualTo('/'), LexCqlToken.Slash)
    .Match(Span.EqualTo("!="), LexCqlToken.NotEquals)
    .Match(Span.EqualTo("=="), LexCqlToken.DoubleEquals)
    .Match(Character.EqualTo('='), LexCqlToken.Equals)
    .Match(QuotedStringToken, LexCqlToken.QuotedString)
    .Match(IdentifierToken, LexCqlToken.Identifier, true)
    .Build();
}