using Superpower.Display;

namespace IDS.TextPlus.FCSEndpoint.Traslator.LexCql;

/// <summary>
///   Token kinds for the LexCQL query language.
/// </summary>
public enum LexCqlToken
{
  [Token(Category = "identifier")] Identifier,

  [Token(Category = "string", Example = "\"hello\"")]
  QuotedString,

  [Token(Category = "operator", Example = "=")]
  Equals,

  [Token(Category = "operator", Example = "==")]
  DoubleEquals,

  [Token(Category = "operator", Example = "!=")]
  NotEquals,

  [Token(Category = "keyword", Example = "AND")]
  And,

  [Token(Category = "keyword", Example = "OR")]
  Or,

  [Token(Category = "keyword", Example = "NOT")]
  Not,

  [Token(Category = "punctuation", Example = "(")]
  LParen,

  [Token(Category = "punctuation", Example = ")")]
  RParen,

  [Token(Category = "modifier", Example = "/")]
  Slash
}