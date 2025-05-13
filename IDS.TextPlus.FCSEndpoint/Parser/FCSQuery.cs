using Antlr4.Runtime;

namespace IDS.TextPlus.FCSEndpoint.Parser;

public static class FCSQuery
{
  public static FCSParser Parse(string query)
  {
    var ais = new AntlrInputStream(query.Replace("[", "(").Replace("]", ")"));
    var lexer = new FCSLexer(ais);
    var cts = new CommonTokenStream(lexer);
    var res = new FCSParser(cts);
    //res.RemoveErrorListeners();
    return res;
  }
}