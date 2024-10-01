using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDS.TextPlus.FCSEndpoint.Parser
{
  public static class FCSQuery
  {
    public static FCSParser Parse(string query)
    {
      var ais = new AntlrInputStream(query);
      var lexer = new FCSLexer(ais);
      var cts = new CommonTokenStream(lexer);
      var res = new FCSParser(cts);
      //res.RemoveErrorListeners();
      return res;
    }
  }
}
