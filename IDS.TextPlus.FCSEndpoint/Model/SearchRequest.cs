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

    public string[] attributesToHighlight { get; set; } = null;
    public string highlightPreTag { get; private set; } = "<hits:Hit>";
    public string highlightPostTag { get; private set; } = "</hits:Hit>";
  }
}
