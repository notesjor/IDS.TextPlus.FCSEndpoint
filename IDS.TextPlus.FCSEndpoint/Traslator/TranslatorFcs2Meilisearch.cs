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

      var dic = new Dictionary<string, string>
      {
        { "text", null },
        { "lemma", null },
        { "pos", null },
        { "source", null }
      };

      res.Query = "";
      var currentKey = "";
      var next = 0;

      for (var i = 0; i < mq.ChildCount; i++)
      {
        var token = mq.GetChild(i).GetText();
        if (_ingore.Contains(token))
          continue;

        if (next == 0)
        {
          if (dic.ContainsKey(token))
          {
            currentKey = token;
            if (dic[currentKey] == null)
            {
              next = 2;
              dic[currentKey] = "";
            }
            else
            {
              var nt = mq.GetChild(++i).GetText();
              dic[currentKey] = nt switch
              {
                "=" => $"{dic[currentKey]} AND",
                "!=" => $"{dic[currentKey]} NOT",
                _ => dic[currentKey]
              };
            }
          }
          else
          {
            res.Query = $"{res.Query} {token}";
          }
        }
        else
        {
          dic[currentKey] = $"{dic[currentKey]} {token}";
          next--;
        }
      }

      if (res.Query.Length > 0)
        res.Query = res.Query.Trim();
      if(res.Query.EndsWith("AND"))
        res.Query = res.Query.Length >= 4 ? res.Query.Substring(0, res.Query.Length - 4) : "*";
      if(res.Query.EndsWith("OR"))
        res.Query = res.Query.Length >= 3 ? res.Query.Substring(0, res.Query.Length - 3) : "*";
      res.Filter = string.Join(" AND ",
        dic.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value.Trim()).Select(x => $"{x.Key} {x.Value}"));
      if (res.Filter == "")
        res.Filter = null;
      if (string.IsNullOrWhiteSpace(res.Query))
        res.Query = "*";
      return res;
    }
  }
}
