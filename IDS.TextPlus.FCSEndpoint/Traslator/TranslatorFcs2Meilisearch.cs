using IDS.TextPlus.FCSEndpoint.Parser;
using System.Runtime.CompilerServices;

namespace IDS.TextPlus.FCSEndpoint.Traslator
{
  public class TranslateFcs2Meilisearch
  {
    private static readonly HashSet<string> _ingore = new() { "(", ")" };
    private static readonly HashSet<string> choice = new HashSet<string> { "AND", "OR" };
    private static readonly HashSet<string> allowFulltextsearch = new HashSet<string>() { "lemma", "lemma_fcs", "link", "hyperonym", "hyponym", "antonym", "synonym", "definition", "citation" };

    private TranslateFcs2Meilisearch() { }

    public string Query { get; set; }
    public string Filter { get; set; }
    public HashSet<string> AttributesToSearchOn { get; set; } = new HashSet<string> { "lemma", "lemma_fcs" };

    public static TranslateFcs2Meilisearch Create(FCSParser.Main_queryContext mq)
    {
      var res = new TranslateFcs2Meilisearch();
      if (mq.ChildCount == 1)
      {
        res.Query = mq.GetChild(0).GetText();
        return res;
      }

      var facet = new Dictionary<string, string>
      {
        { "lemma", null },
        { "entryId", null },
        { "source", null },
        { "senseRef", null },
        { "lang", null },
        { "pos", null },
        { "definition", null },
        { "segmentation", null },
        { "gender", null },
        { "number", null },
        { "citation", null },
        { "link", null },
        { "synonym", null },
        { "antonym", null },
        { "hyponym", null },
        { "hypernym", null }
      };

      var isChoiceMode = false;
      var choices = new List<string>();
      var choiceOperator = "";

      var hasFacettes = false;

      res.Query = "";
      var currentKey = "";
      var next = 0;
      string lastChoice = " ";

      for (var i = 0; i < mq.ChildCount; i++)
      {
        var token = mq.GetChild(i).GetText();
        if (_ingore.Contains(token))
          continue;

        if (next == 0)
        {
          if (facet.ContainsKey(token))
          {
            hasFacettes = true;

            currentKey = token;
            if (facet[currentKey] == null)
            {
              next = 2;
              facet[currentKey] = "";
            }
            else
            {
              i += 2;
              var nt = mq.GetChild(i).GetText();
              facet[currentKey] = $"{facet[currentKey]}{lastChoice}{nt}";
            }
          }
          else
          {
            if (choice.Contains(token))
            {
              isChoiceMode = true;
              choiceOperator = token;
              lastChoice = $" {token} ";
              continue;
            }

            res.Query = $"{res.Query}{lastChoice}{token}";
            choices.Add(token);
          }
        }
        else
        {
          if (lastChoice != " ")
          {
            res.Query = $"{res.Query}{lastChoice}{token}";
            choices.Add(token);
          }
          else
            facet[currentKey] = $"{facet[currentKey]} {token}";
          next--;
        }
        lastChoice = " ";
      }

      foreach (var f in facet.Where(x => !string.IsNullOrWhiteSpace(x.Value) && allowFulltextsearch.Contains(x.Key)))
        res.AttributesToSearchOn.Add(f.Key);

      res.Filter = string.Join(" AND ",
        facet.Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToDictionary(x => x.Key, x => x.Value.Trim()).Select(x => $"{x.Key} {x.Value}"));

      if (!hasFacettes && isChoiceMode && choices.Count > 0)
      {
        res.Query = "*";
        res.Filter = $"{(!string.IsNullOrWhiteSpace(res.Filter) ? $"{res.Filter} AND " : "")} {string.Join($" {choiceOperator} ", choices.Select(x => $"lemma_token = \"{x.Replace("\"", "")}\""))}".Trim();
      }

      if (res.Query.Length > 0)
        res.Query = res.Query.Trim();
      if (res.Query.EndsWith("AND"))
        res.Query = res.Query.Length >= 4 ? res.Query.Substring(0, res.Query.Length - 4) : "*";
      if (res.Query.EndsWith("OR"))
        res.Query = res.Query.Length >= 3 ? res.Query.Substring(0, res.Query.Length - 3) : "*";
      if (string.IsNullOrWhiteSpace(res.Filter))
        res.Filter = null;
      if (string.IsNullOrWhiteSpace(res.Query))
        res.Query = "*";
      return res;
    }
  }
}
