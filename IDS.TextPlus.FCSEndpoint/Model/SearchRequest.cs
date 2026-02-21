using IDS.TextPlus.FCSEndpoint.Traslator;
using Newtonsoft.Json.Linq;

namespace IDS.TextPlus.FCSEndpoint.Model;

/// <summary>
/// Builds an Elasticsearch search request body from a LexCQL query.
/// </summary>
public class SearchRequest
{
  private JToken? _query;

  public int From { get; set; }
  public int Size { get; set; } = 10;

  /// <summary>
  /// Parses a LexCQL query string and builds the Elasticsearch query.
  /// </summary>
  public void SetQuery(string query)
  {
    var translator = TranslatorFcs2Elasticsearch.Parse(query);
    _query = JToken.Parse(translator.ElasticsearchQuery.ToJsonString());
  }

  /// <summary>
  /// Wraps the current query with an additional term filter for source.
  /// </summary>
  public void AddSourceFilter(string source)
  {
    _query = new JObject
    {
      ["bool"] = new JObject
      {
        ["must"] = new JArray { _query },
        ["filter"] = new JArray
        {
          new JObject
          {
            ["term"] = new JObject { ["source"] = new JObject { ["value"] = source } }
          }
        }
      }
    };
  }

  /// <summary>
  /// Serializes the complete Elasticsearch request body to JSON.
  /// </summary>
  public string ToRequestJson()
  {
    var body = new JObject
    {
      ["query"] = _query?.DeepClone(),
      ["from"] = From,
      ["size"] = Size,
      ["highlight"] = new JObject
      {
        ["pre_tags"] = new JArray { "<hits:Hit>" },
        ["post_tags"] = new JArray { "</hits:Hit>" },
        ["fields"] = new JObject
        {
          ["lemma"] = new JObject(),
          ["text"] = new JObject()
        }
      }
    };

    return body.ToString(Newtonsoft.Json.Formatting.None);
  }
}