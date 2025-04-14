using IDS.TextPlus.FCSEndpoint.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace IDS.TextPlus.FCSEndpoint.Traslator
{
  public class TranslateFcs2Meilisearch
  {
    private static readonly HashSet<string> _facetKeys = new() { "text", "lemma", "pos", "source" };
    private static readonly HashSet<string> _logicalOperators = new() { "AND", "OR", "NOT" };

    private TranslateFcs2Meilisearch() { }

    public string Query { get; private set; }
    public string Filter { get; private set; }

    public static TranslateFcs2Meilisearch Create(FCSParser.Main_queryContext mq)
    {
      var translator = new TranslateFcs2Meilisearch();
      var queryBuilder = new StringBuilder();
      var filterBuilder = new StringBuilder();

      translator.ProcessContext(mq, ref queryBuilder, ref filterBuilder);

      translator.Query = string.IsNullOrWhiteSpace(queryBuilder.ToString()) ? "*" : queryBuilder.ToString().Trim();
      translator.Filter = string.IsNullOrWhiteSpace(filterBuilder.ToString()) ? null : filterBuilder.ToString().Trim();

      return translator;
    }

    private void ProcessContext(ParserRuleContext context, ref StringBuilder queryBuilder,
      ref StringBuilder filterBuilder)
    {
      for (var i = 0; i < context.ChildCount; i++)
      {
        var child = context.GetChild(i) as TerminalNodeImpl;
        if (child != null)
        {
          ProcessToken(child, ref queryBuilder, ref filterBuilder);
        }
      }
    }

    private void ProcessToken(TerminalNodeImpl token, ref StringBuilder queryBuilder, ref StringBuilder filterBuilder)
    {

      /* Fehlerhaft
      switch (token)
      {
        case FCSParser.Query_disjunctionContext disjunction:
          ProcessDisjunction(disjunction, ref queryBuilder, ref filterBuilder);
          break;
        case FCSParser.Query_sequenceContext sequence:
          ProcessSequence(sequence, ref queryBuilder, ref filterBuilder);
          break;
        case FCSParser.Query_groupContext group:
          ProcessGroup(group, ref queryBuilder, ref filterBuilder);
          break;
        case FCSParser.Query_simpleContext simple:
          ProcessSimple(simple, ref queryBuilder, ref filterBuilder);
          break;
        default:
          break;
      }*/
    }

    private void ProcessDisjunction(FCSParser.Query_disjunctionContext disjunction, ref StringBuilder queryBuilder,
      ref StringBuilder filterBuilder)
    {
      var localQuery = new StringBuilder();
      var localFilter = new StringBuilder();

      for (var i = 0; i < disjunction.ChildCount; i++)
      {
        var child = disjunction.GetChild(i) as TerminalNodeImpl;
        if (child != null)
        {
          ProcessToken(child, ref localQuery, ref localFilter);
        }

        if (i < disjunction.ChildCount - 1)
        {
          localQuery.Append(" OR ");
          localFilter.Append(" OR ");
        }
      }

      AppendToBuilder(ref queryBuilder, localQuery.ToString());
      AppendToBuilder(ref filterBuilder, localFilter.ToString());
    }

    private void ProcessSequence(FCSParser.Query_sequenceContext sequence, ref StringBuilder queryBuilder,
      ref StringBuilder filterBuilder)
    {
      var localQuery = new StringBuilder();
      var localFilter = new StringBuilder();

      for (var i = 0; i < sequence.ChildCount; i++)
      {
        var child = sequence.GetChild(i) as TerminalNodeImpl;
        if (child != null)
        {
          ProcessToken(child, ref localQuery, ref localFilter);
        }

        if (i < sequence.ChildCount - 1)
        {
          localQuery.Append(" AND ");
          localFilter.Append(" AND ");
        }
      }

      AppendToBuilder(ref queryBuilder, localQuery.ToString());
      AppendToBuilder(ref filterBuilder, localFilter.ToString());
    }

    private void ProcessGroup(FCSParser.Query_groupContext group, ref StringBuilder queryBuilder,
      ref StringBuilder filterBuilder)
    {
      var localQuery = new StringBuilder();
      var localFilter = new StringBuilder();

      for (var i = 0; i < group.ChildCount; i++)
      {
        var child = group.GetChild(i) as TerminalNodeImpl;
        if (child != null)
        {
          ProcessToken(child, ref localQuery, ref localFilter);
        }
      }

      if (localQuery.Length > 0)
      {
        queryBuilder.Append("(").Append(localQuery).Append(")");
      }

      AppendToBuilder(ref filterBuilder, localFilter.ToString());
    }

    private void ProcessSimple(FCSParser.Query_simpleContext simple, ref StringBuilder queryBuilder,
      ref StringBuilder filterBuilder)
    {
      var text = simple.GetText();

      if (_facetKeys.Any(facet => text.StartsWith(facet + " =") || text.StartsWith(facet + " !=") || text.StartsWith(facet + " NOT")))
      {
        // Handle "NOT" as "!=" for invalid FCS syntax
        if (text.Contains(" NOT "))
        {
          text = text.Replace(" NOT ", " != ");
        }

        AppendToBuilder(ref filterBuilder, text);
      }
      else
      {
        AppendToBuilder(ref queryBuilder, text);
      }
    }

    private void AppendToBuilder(ref StringBuilder builder, string value)
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        if (builder.Length > 0)
        {
          builder.Append(" ");
        }
        builder.Append(value);
      }
    }
  }
}
