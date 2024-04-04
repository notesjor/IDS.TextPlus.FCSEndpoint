using IDS.TextPlus.FCSEndpoint.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDS.TextPlus.FCSEndpoint.Model
{
  public class SearchRequest
  {
    public string q { get; set; }
    public int limit { get; set; }
    public int offset { get; set; }
    public string filter { get; set; }

    public string[] attributesToHighlight { get; set; } = new[] { "lemma", "def" };
    public string highlightPreTag { get; set; } = "<hits:Hit>";
    public string highlightPostTag { get; set; } = "</hits:Hit>";

    private static HashSet<string> _ingore = new HashSet<string> { "(", ")" };

    public void SetQuery(string query)
    {
      var parts = FCSQuery.Parse(query);
      var mq = parts.main_query();
      if (mq.ChildCount == 1)
      {
        q = mq.GetChild(0).GetText();
        return;
      }

      var dic = new Dictionary<string, string>
      {
        {"lemma", null},
        {"pos", null},
        {"source", null}
      };

      q = "";
      var currentKey = "";
      var next = 0;

      for (var i = 0; i < mq.ChildCount; i++)
      {
        var token = mq.GetChild(i).GetText();
        if (_ingore.Contains(token))
          continue;

        if(next == 0)
        {
          if(dic.ContainsKey(token))
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
              if(nt == "=")
                dic[currentKey] = $"{dic[currentKey]} AND";
              if (nt == "!=")
                dic[currentKey] = $"{dic[currentKey]} NOT";
            }
          }
          else
          {
            q = $"{q} {token}";
          }
        }
        else
        {
          dic[currentKey] = $"{dic[currentKey]} {token}";
          next--;
        }
      }

      filter = string.Join(" ", dic.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value.Trim()).Select(x => $"{x.Key} {x.Value}"));
    }
  }
}
