using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDS.TextPlus.FCSEndpoint.Model
{
  public class SearchResponseContainer
  {
    [JsonProperty("_formatted")]
    public IDS.TextPlus.FCSEndpoint.Model.SearchResult Formatted { get; set; }
  }
}
