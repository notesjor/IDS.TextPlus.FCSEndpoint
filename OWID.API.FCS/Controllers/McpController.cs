using IDS.TextPlus.FCSEndpoint.Version;
using IDS.TextPlus.FCSEndpoint.Model;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace OWID.API.FCS.Controllers
{
  [ApiController]
  [Route("/llm")]
  [Produces("application/json")]
  [McpServerToolType]
  public class McpController : ControllerBase
  {
    private static readonly Version20 DefaultVersion = new Version20();
    private static readonly HttpClient _client = new HttpClient();

    [McpServerTool(Destructive = false, Name = "explain_data", OpenWorld = false, OutputSchemaType = typeof(explainResponse), ReadOnly = true, Title = "FCS-Explain Document")]
    [Description("Returns a JSON document that describes the FCS endpoint and its capabilities.")]
    [ProducesResponseType(typeof(explainResponse), 200)]
    public async Task<explainResponse> ExplainData()
    {
#if DEBUG
      Console.WriteLine("explain_data called");
#endif
      return DefaultVersion.SendExplainBypass();
    }

    [McpServerTool(Destructive = false, Name = "search_data", OpenWorld = false, OutputSchemaType = typeof(string), ReadOnly = true, Title = "FCS-Search v2.0")]
    [Description("Search the FCS endpoint by supplying any combination of search criteria. All supplied criteria are combined with AND. Use * or ? in lemma for wildcard matching.")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<string> SearchData(
      [Description("Lemma to search for. Supports * and ? wildcards, e.g. *en.")] string? lemma = null,
      [Description("Part of speech, e.g. NOUN.")] string? pos = null,
      [Description("Grammatical gender, e.g. Masc.")] string? gender = null,
      [Description("Grammatical number, e.g. Sing.")] string? number = null,
      [Description("Language code.")] string? lang = null,
      [Description("Resource/source identifier.")] string? source = null,
      [Description("Text to search in definitions.")] string? definition = null,
      [Description("Text to search in citations.")] string? citation = null,
      [Description("Text to search in segmentation.")] string? segmentation = null,
      [Description("Related lemma to search for as a synonym.")] string? synonym = null,
      [Description("Related lemma to search for as an antonym.")] string? antonym = null,
      [Description("Related lemma to search for as a hyperonym.")] string? hypernym = null,
      [Description("Related lemma to search for as a hyponym.")] string? hyponym = null,
      [Description("Entry identifier.")] string? entryId = null,
      [Description("Sense identifier.")] string? senseRef = null,
      [Description("Maximum number of results to return (1-100).")] int maximumRecords = 10)
    {
      try
      {
        if (maximumRecords is < 1 or > 100)
          return "maximumRecords must be between 1 and 100.";

        var criteria = new List<string>();
        AddCriterion(criteria, "lemma", lemma);
        AddCriterion(criteria, "pos", pos, exact: true);
        AddCriterion(criteria, "gender", gender, exact: true);
        AddCriterion(criteria, "number", number, exact: true);
        AddCriterion(criteria, "lang", lang, exact: true);
        AddCriterion(criteria, "source", source, exact: true);
        AddCriterion(criteria, "definition", definition);
        AddCriterion(criteria, "citation", citation);
        AddCriterion(criteria, "segmentation", segmentation);
        AddCriterion(criteria, "synonym", synonym);
        AddCriterion(criteria, "antonym", antonym);
        AddCriterion(criteria, "hyperonym", hypernym);
        AddCriterion(criteria, "hyponym", hyponym);
        AddCriterion(criteria, "entryId", entryId, exact: true);
        AddCriterion(criteria, "senseRef", senseRef, exact: true);

        if (criteria.Count == 0)
          return "At least one search criterion must be provided.";

        var searchRequest = new SearchRequest { Size = maximumRecords };
        searchRequest.SetQuery(string.Join(" AND ", criteria));
        using var content = new StringContent(searchRequest.ToRequestJson(), Encoding.UTF8, "application/json");
#if DEBUG
        Console.WriteLine("search_data called");
        Console.WriteLine(searchRequest.ToRequestJson());
#endif
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        using var response = await _client.PostAsync("http://localhost:9200/fcs/_search", content);

#if DEBUG
        var res = await response.Content.ReadAsStringAsync();
        Console.WriteLine(res);
        return res;
#else
        return await response.Content.ReadAsStringAsync();
#endif

        return await response.Content.ReadAsStringAsync();
      }
      catch
      {
        return "Error occurred while executing the ElasticSearch query.";
      }
    }

    private static void AddCriterion(List<string> criteria, string field, string? value, bool exact = false)
    {
      if (string.IsNullOrWhiteSpace(value))
        return;

      var escapedValue = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
      criteria.Add($"{field} {(exact ? "==" : "=")} \"{escapedValue}\"");
    }
  }
}
