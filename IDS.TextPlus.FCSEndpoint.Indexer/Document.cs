using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDS.TextPlus.FCSEndpoint.Indexer
{
  public class Document
  {
    public string id { get; set; }
    public string url { get; set; }
    public string source { get; set; }

    public string lemma { get; set; }
    public string text { get; set; }
  }
}
