using IDS.TextPlus.FCSEndpoint.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace IDS.TextPlus.FCSEndpoint.Traslator
{
  public class TranslateFcs2Meilisearch
  {
    private static readonly Dictionary<string, string> _facetKeys = new()
          {
              { "text", null },
              { "lemma", null },
              { "pos", null },
              { "source", null }
          };

    private TranslateFcs2Meilisearch() { }

    public string Query { get; private set; }
    public string Filter { get; private set; }

    public static TranslateFcs2Meilisearch Create(FCSParser.Main_queryContext mq)
    {
      var translator = new TranslateFcs2Meilisearch();
      var queryBuilder = new StringBuilder();
      var filterBuilder = new StringBuilder();

      for (var i = 0; i < mq.ChildCount; i++)
      {
        var token = mq.children[i] as ParserRuleContext;
        if (token != null)
        {
          translator.ProcessToken(token, queryBuilder, filterBuilder);
        }
      }

      translator.Query = queryBuilder.ToString();
      translator.Filter = filterBuilder.ToString();

      return translator;
    }

    private void ProcessToken(ParserRuleContext token, StringBuilder queryBuilder, StringBuilder filterBuilder)
    {
      switch (token)
      {
        case FCSParser.Query_disjunctionContext disjunction:
          ProcessDisjunction(disjunction, queryBuilder, filterBuilder);
          break;
        case FCSParser.Query_sequenceContext sequence:
          ProcessSequence(sequence, queryBuilder, filterBuilder);
          break;
        case FCSParser.Query_groupContext group:
          ProcessGroup(group, queryBuilder, filterBuilder);
          break;
        case FCSParser.Query_simpleContext simple:
          ProcessSimple(simple, queryBuilder, filterBuilder);
          break;
        default:
          break;
      }
    }

    private void ProcessDisjunction(FCSParser.Query_disjunctionContext disjunction, StringBuilder queryBuilder, StringBuilder filterBuilder)
    {
      for (var i = 0; i < disjunction.ChildCount; i++)
      {
        var child = disjunction.children[i];
        ProcessToken(child as ParserRuleContext, queryBuilder, filterBuilder);
        if (i < disjunction.ChildCount - 1)
        {
          queryBuilder.Append(" OR ");
        }
      }
    }

    private void ProcessSequence(FCSParser.Query_sequenceContext sequence, StringBuilder queryBuilder, StringBuilder filterBuilder)
    {
      for (var i = 0; i < sequence.ChildCount; i++)
      {
        var child = sequence.children[i];
        ProcessToken(child as ParserRuleContext, queryBuilder, filterBuilder);
        if (i < sequence.ChildCount - 1)
        {
          queryBuilder.Append(" ");
        }
      }
    }

    private void ProcessGroup(FCSParser.Query_groupContext group, StringBuilder queryBuilder, StringBuilder filterBuilder)
    {
      queryBuilder.Append("(");
      for (var i = 0; i < group.ChildCount; i++)
      {
        var child = group.children[i];
        ProcessToken(child as ParserRuleContext, queryBuilder, filterBuilder);
      }
      queryBuilder.Append(")");
    }

    private void ProcessSimple(FCSParser.Query_simpleContext simple, StringBuilder queryBuilder, StringBuilder filterBuilder)
    {
      var text = simple.GetText();
      if (_facetKeys.ContainsKey(text))
      {
        filterBuilder.Append(text);
      }
      else
      {
        queryBuilder.Append(text);
      }
    }
  }
}
