using IDS.TextPlus.FCSEndpoint.Parser;

namespace IDS.TextPlus.FCSEndpoint.Model;

public class SearchRequest
{
  private static readonly HashSet<string> _ingore = new() { "(", ")" };
  public string q { get; set; }
  public int limit { get; set; } = 10;
  public int offset { get; set; } = 0;
  public string filter { get; set; }

  public string[] attributesToHighlight { get; set; } = { "lemma", "text" };
  public string highlightPreTag { get; set; } = "<hits:Hit>";
  public string highlightPostTag { get; set; } = "</hits:Hit>";

  public void SetQuery(string query)
  {
    var parts = FCSQuery.Parse(query);
    if (parts.NumberOfSyntaxErrors > 0)
      throw new TypeLoadException("Invalid query");

    var mq = parts.main_query();
    if (mq.ChildCount == 1)
    {
      q = mq.GetChild(0).GetText();
      return;
    }

    var dic = new Dictionary<string, string>
    {
      { "lemma", null },
      { "pos", null },
      { "source", null }
    };

    q = "";
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
            if (nt == "=")
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

    filter = string.Join(" ",
      dic.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value.Trim()).Select(x => $"{x.Key} {x.Value}"));
  }
}