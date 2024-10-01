using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDS.TextPlus.FCSEndpoint.Model
{
  public class SearchResult
  {
    [JsonProperty("source")]
    public string Source { get; set; }
    [JsonProperty("lemma")]
    public string Lemma { get; set; }
    [JsonProperty("pos")]
    public string Pos { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }    
  }
}
