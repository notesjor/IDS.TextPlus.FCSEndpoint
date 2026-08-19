using System.Text.Json;
using System.Text.Json.Nodes;
using IDS.TextPlus.FCSEndpoint.Traslator;

namespace IDS.TextPlus.FCSEndpoint.Model;

/// <summary>
///   Builds an Elasticsearch search request body from a LexCQL query.
/// </summary>
public class SearchRequest
{
  private JsonNode? _query;

  public int From { get; set; }
  public int Size { get; set; } = 10;

  /// <summary>
  ///   Parses a LexCQL query string and builds the Elasticsearch query.
  /// </summary>
  public void SetQuery(string query)
  {
    var translator = TranslatorFcs2Elasticsearch.Parse(query);
    var json = translator.ElasticsearchQuery.ToJsonString();
    _query = string.IsNullOrWhiteSpace(json) ? null : JsonNode.Parse(json);
  }

  /// <summary>
  ///   Wraps the current query with an additional term filter for source.
  /// </summary>
  public void AddSourceFilter(string source)
  {
    // Build equivalent of: { "bool": { "must": [ _query ], "filter": [ { "term": { "source": { "value": source } } } ] } }
    var mustArray = new JsonArray();
    if (_query != null)
      mustArray.Add(_query.DeepClone());

    var filterArray = new JsonArray
    {
      new JsonObject
      {
        ["term"] = new JsonObject
        {
          ["source"] = new JsonObject
          {
            ["value"] = source
          }
        }
      }
    };

    _query = new JsonObject
    {
      ["bool"] = new JsonObject
      {
        ["must"] = mustArray,
        ["filter"] = filterArray
      }
    };
  }

  /// <summary>
  ///   Serializes the complete Elasticsearch request body to JSON.
  /// </summary>
  public string ToRequestJson()
  {
    var body = new JsonObject
    {
      ["query"] = _query?.DeepClone() ?? new JsonObject(),
      ["from"] = From,
      ["size"] = Size,
      ["highlight"] = new JsonObject
      {
        ["pre_tags"] = new JsonArray { "<hits:Hit>" },
        ["post_tags"] = new JsonArray { "</hits:Hit>" },
        ["require_field_match"] = false,
        ["fragment_size"] = 1000,
        ["fields"] = new JsonObject
        {
          ["text"] = new JsonObject(),
          ["lemma"] = new JsonObject()
        }
      }
    };

    // Serialize preserving the explicit property names
    return body.ToJsonString(new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
  }
}