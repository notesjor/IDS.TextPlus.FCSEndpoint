using IDS.TextPlus.FCSEndpoint.Parser;

namespace IDS.TextPlus.FCSEndpoint.Traslator
{
  public class TranslateFcs2Meilisearch
  {
    private static readonly HashSet<string> _ingore = new() { "(", ")" };

    private TranslateFcs2Meilisearch() { }

    public string Query { get; set; }
    public string Filter { get; set; }

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
        //{ "text", null },
        { "lemma", null },
        { "id", null },
        { "entryId", null },
        { "senseRef", null },
        { "source", null }, 
        { "lang", null }, 
        { "gender", null }, 
        { "number", null },         
        { "pos", null },
        { "related", null },    
        { "hyperonym", null }, 
        { "hyponym", null }, 
        { "antonym", null }, 
        { "synonym", null }, 
      };
      var choice = new HashSet<string> { "AND", "OR" };

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
            currentKey = token;
            if (facet[currentKey] == null)
            {
              next = 2;
              facet[currentKey] = "";
            }
            else
            {
              i+=2;
              var nt = mq.GetChild(i).GetText();
              facet[currentKey] = $"{facet[currentKey]}{lastChoice}{nt}";
            }
          }
          else
          {
            if (choice.Contains(token))
            {
              lastChoice = $" {token} ";
              continue;
            }

            res.Query = $"{res.Query}{lastChoice}{token}";
          }
        }
        else
        {
          if (lastChoice != " ")
            res.Query = $"{res.Query}{lastChoice}{token}";
          else
            facet[currentKey] = $"{facet[currentKey]} {token}";
          next--;
        }
        lastChoice = " ";
      }

      if (res.Query.Length > 0)
        res.Query = res.Query.Trim();
      if (res.Query.EndsWith("AND"))
        res.Query = res.Query.Length >= 4 ? res.Query.Substring(0, res.Query.Length - 4) : "*";
      if (res.Query.EndsWith("OR"))
        res.Query = res.Query.Length >= 3 ? res.Query.Substring(0, res.Query.Length - 3) : "*";
      res.Filter = string.Join(" AND ",
        facet.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value.Trim()).Select(x => $"{x.Key} {x.Value}"));
      if (res.Filter == "")
        res.Filter = null;
      if (string.IsNullOrWhiteSpace(res.Query))
        res.Query = "*";
      return res;
    }
  }
}
