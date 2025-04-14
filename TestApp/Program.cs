using Antlr4.Runtime.Tree;
using IDS.TextPlus.FCSEndpoint.Parser;
using IDS.TextPlus.FCSEndpoint.Traslator;

namespace TestApp
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var types = new Dictionary<int, HashSet<string>>();

      Wrap("fu", ref types);
      Wrap("pandemie OR corona", ref types);
      Wrap("pandemie AND NOT corona", ref types);
      Wrap("fu AND source = neo", ref types);
      Wrap("fu AND source != neo", ref types);
      Wrap("fu AND source NOT neo", ref types);
      Wrap("(pandemie AND corona)", ref types);
      Wrap("Auge AND source = neo OR source = sprw", ref types);
      Wrap("lemma = corona AND text = pandemie", ref types);
    }

    private static void Wrap(string query, ref Dictionary<int, HashSet<string>> types)
    {
      var context = FCSQuery.Parse(query).main_query();
      for (var i = 0; i < context.ChildCount; i++)
      {
        var child = context.GetChild(i) as TerminalNodeImpl;
        if (child != null)
        {
          var type = child.Symbol.Type;
          if (!types.ContainsKey(type))
            types[type] = new HashSet<string>();
          types[type].Add(child.Payload.Text);
        }
        else
          throw new Exception();
      }
    }
  }
}
