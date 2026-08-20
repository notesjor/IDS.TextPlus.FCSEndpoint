using IDS.TextPlus.FCSEndpoint.Version;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OWID.API.FCS.Controllers
{
  [ApiController]
  [Route("/llm")]
  [Produces("application/json")]
  [McpServerToolType]
  public class McpController : ControllerBase
  {
    private const string ElasticsearchEndpoint = "http://localhost:9200/fcs/_search";
    private const string CatalogPath = "Snippets/catalog.json";

    private static readonly Version20 DefaultVersion = new Version20();
    private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(30) };

    /// <summary>
    /// Returns the FCS explain document.
    /// </summary>
    [McpServerTool(Destructive = false, Name = "explain_data", OpenWorld = false, OutputSchemaType = typeof(explainResponse), ReadOnly = true, Title = "FCS-Explain Document")]
    [Description("Returns a JSON document that describes the FCS endpoint and its capabilities.")]
    [ProducesResponseType(typeof(explainResponse), (int)HttpStatusCode.OK)]
    public Task<explainResponse> ExplainData()
    {
#if DEBUG
      Console.WriteLine("explain_data called");
#endif
      return Task.FromResult(DefaultVersion.SendExplainBypass());
    }

    /// <summary>
    /// Executes a raw ElasticSearch query against the FCS index.
    /// </summary>
    [McpServerTool(Destructive = false, Name = "search_data", OpenWorld = false, OutputSchemaType = typeof(McpSearchResponse), ReadOnly = true, Title = "FCS-Raw ElasticSearch")]
    [Description("Executes a raw ElasticSearch query JSON against the FCS index.")]
    [ProducesResponseType(typeof(McpSearchResponse), (int)HttpStatusCode.OK)]
    public Task<McpSearchResponse> SearchData([Description("The ElasticSearch query JSON to execute.")] string elasticSearchQuery, CancellationToken cancellationToken)
      => ExecuteElasticQueryAsync(elasticSearchQuery, cancellationToken);

    /// <summary>
    /// Searches by lemma and optional source key.
    /// </summary>
    [McpServerTool(Destructive = false, Name = "search_by_lemma", OpenWorld = false, OutputSchemaType = typeof(McpSearchResponse), ReadOnly = true, Title = "FCS-Search by Lemma")]
    [Description("Builds and executes a useful default query for the lemma field.")]
    [ProducesResponseType(typeof(McpSearchResponse), (int)HttpStatusCode.OK)]
    public Task<McpSearchResponse> SearchByLemma(
      [Description("The lemma value to search for.")] string lemma,
      [Description("Result offset.")] int from = 0,
      [Description("Result size (1..100).") ] int size = 10,
      [Description("Optional source key from list_sources (e.g. sprw, neo, elex).") ] string? source = null,
      CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrWhiteSpace(lemma))
      {
        return Task.FromResult(McpSearchResponse.CreateValidationError("lemma must not be empty."));
      }

      if (!TryValidatePagination(from, size, out var paginationError))
      {
        return Task.FromResult(McpSearchResponse.CreateValidationError(paginationError));
      }

      var query = BuildLemmaQuery(lemma, from, size, source);
      return ExecuteElasticQueryAsync(query, cancellationToken);
    }

    /// <summary>
    /// Searches by lemma regex and optional source key.
    /// </summary>
    [McpServerTool(Destructive = false, Name = "search_by_lemma_regex", OpenWorld = false, OutputSchemaType = typeof(McpSearchResponse), ReadOnly = true, Title = "FCS-Search by Lemma Regex")]
    [Description("Builds and executes a regex query for lemma.keyword.")]
    [ProducesResponseType(typeof(McpSearchResponse), (int)HttpStatusCode.OK)]
    public Task<McpSearchResponse> SearchByLemmaRegex(
      [Description("Regex for lemma.keyword (e.g. Glück.*).") ] string lemmaRegex,
      [Description("Result offset.")] int from = 0,
      [Description("Result size (1..100).") ] int size = 10,
      [Description("Optional source key from list_sources (e.g. sprw, neo, elex).") ] string? source = null,
      CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrWhiteSpace(lemmaRegex))
      {
        return Task.FromResult(McpSearchResponse.CreateValidationError("lemmaRegex must not be empty."));
      }

      if (!TryValidatePagination(from, size, out var paginationError))
      {
        return Task.FromResult(McpSearchResponse.CreateValidationError(paginationError));
      }

      var query = BuildLemmaRegexQuery(lemmaRegex, from, size, source);
      return ExecuteElasticQueryAsync(query, cancellationToken);
    }

    /// <summary>
    /// Searches within one source key.
    /// </summary>
    [McpServerTool(Destructive = false, Name = "search_by_source", OpenWorld = false, OutputSchemaType = typeof(McpSearchResponse), ReadOnly = true, Title = "FCS-Search by Source")]
    [Description("Builds and executes a source-filter query.")]
    [ProducesResponseType(typeof(McpSearchResponse), (int)HttpStatusCode.OK)]
    public Task<McpSearchResponse> SearchBySource(
      [Description("Source key from list_sources (e.g. sprw, neo, elex).") ] string source,
      [Description("Result offset.")] int from = 0,
      [Description("Result size (1..100).") ] int size = 10,
      CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrWhiteSpace(source))
      {
        return Task.FromResult(McpSearchResponse.CreateValidationError("source must not be empty."));
      }

      if (!TryValidatePagination(from, size, out var paginationError))
      {
        return Task.FromResult(McpSearchResponse.CreateValidationError(paginationError));
      }

      var query = BuildSourceQuery(source, from, size);
      return ExecuteElasticQueryAsync(query, cancellationToken);
    }

    /// <summary>
    /// Lists available source keys from the local catalog.
    /// </summary>
    [McpServerTool(Destructive = false, Name = "list_sources", OpenWorld = false, OutputSchemaType = typeof(McpSourceListResponse), ReadOnly = true, Title = "FCS-List Sources")]
    [Description("Returns available source keys for source filtering.")]
    [ProducesResponseType(typeof(McpSourceListResponse), (int)HttpStatusCode.OK)]
    public async Task<McpSourceListResponse> ListSources(CancellationToken cancellationToken)
    {
      var catalogFile = Path.Combine(AppContext.BaseDirectory, CatalogPath.Replace('/', Path.DirectorySeparatorChar));
      if (!System.IO.File.Exists(catalogFile))
      {
        return McpSourceListResponse.CreateFailure($"Catalog not found: {catalogFile}");
      }

      try
      {
        await using var stream = System.IO.File.OpenRead(catalogFile);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
          return McpSourceListResponse.CreateFailure("Catalog format is invalid.");
        }

        var sources = document.RootElement.EnumerateObject().Select(x => x.Name).Order().ToArray();
        return McpSourceListResponse.CreateSuccess(sources);
      }
      catch (JsonException ex)
      {
        return McpSourceListResponse.CreateFailure($"Catalog JSON is invalid: {ex.Message}");
      }
      catch (IOException ex)
      {
        return McpSourceListResponse.CreateFailure($"Catalog could not be read: {ex.Message}");
      }
      catch (UnauthorizedAccessException ex)
      {
        return McpSourceListResponse.CreateFailure($"Catalog access denied: {ex.Message}");
      }
    }

    private static bool TryValidatePagination(int from, int size, out string error)
    {
      if (from < 0)
      {
        error = "from must be >= 0.";
        return false;
      }

      if (size is < 1 or > 100)
      {
        error = "size must be between 1 and 100.";
        return false;
      }

      error = string.Empty;
      return true;
    }

    private static string BuildLemmaQuery(string lemma, int from, int size, string? source)
    {
      var payload = new
      {
        query = BuildMustQuery(new object[]
        {
          new
          {
            match = new
            {
              lemma = new
              {
                query = lemma
              }
            }
          }
        }, source),
        from,
        size,
        highlight = CreateDefaultHighlight()
      };

      return JsonSerializer.Serialize(payload);
    }

    private static string BuildLemmaRegexQuery(string lemmaRegex, int from, int size, string? source)
    {
      var payload = new
      {
        query = BuildMustQuery(new object[]
        {
          new
          {
            regexp = new
            {
              lemma = new
              {
                value = lemmaRegex,
                max_determinized_states = 100,
                rewrite = "constant_score"
              }
            }
          }
        }, source),
        from,
        size,
        highlight = CreateDefaultHighlight()
      };

      return JsonSerializer.Serialize(payload);
    }

    private static string BuildSourceQuery(string source, int from, int size)
    {
      var payload = new
      {
        query = BuildMustQuery(Array.Empty<object>(), source),
        from,
        size,
        highlight = CreateDefaultHighlight()
      };

      return JsonSerializer.Serialize(payload);
    }

    private static object BuildMustQuery(IReadOnlyCollection<object> mustClauses, string? source)
    {
      var allMustClauses = mustClauses.ToList();

      if (!string.IsNullOrWhiteSpace(source))
      {
        allMustClauses.Add(new
        {
          @bool = new
          {
            should = new object[]
            {
              new { term = new Dictionary<string, string> { ["source.keyword"] = source } },
              new { term = new Dictionary<string, string> { ["source"] = source } }
            },
            minimum_should_match = 1
          }
        });
      }

      return new
      {
        @bool = new
        {
          must = allMustClauses
        }
      };
    }

    private static object CreateDefaultHighlight()
      => new
      {
        pre_tags = new[] { "<hits:Hit>" },
        post_tags = new[] { "</hits:Hit>" },
        require_field_match = false,
        fragment_size = 1000,
        fields = new
        {
          text = new { },
          lemma = new { }
        }
      };

    private static async Task<McpSearchResponse> ExecuteElasticQueryAsync(string elasticSearchQuery, CancellationToken cancellationToken)
    {
      if (string.IsNullOrWhiteSpace(elasticSearchQuery))
      {
        return McpSearchResponse.CreateValidationError("elasticSearchQuery must not be empty.");
      }

      string normalizedQuery;
      try
      {
        using var queryDoc = JsonDocument.Parse(elasticSearchQuery);
        normalizedQuery = queryDoc.RootElement.GetRawText();
      }
      catch (JsonException ex)
      {
        return McpSearchResponse.CreateValidationError($"elasticSearchQuery is not valid JSON: {ex.Message}");
      }

#if DEBUG
      Console.WriteLine("search_data called");
      Console.WriteLine(normalizedQuery);
#endif

      using var content = new StringContent(normalizedQuery, Encoding.UTF8, "application/json");
      content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

      try
      {
        using var response = await _client.PostAsync(ElasticsearchEndpoint, content, cancellationToken);
        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

#if DEBUG
        Console.WriteLine(rawResponse);
#endif

        if (!response.IsSuccessStatusCode)
        {
          return McpSearchResponse.CreateBackendError((int)response.StatusCode, normalizedQuery, rawResponse);
        }

        try
        {
          using var responseDoc = JsonDocument.Parse(rawResponse);
          return McpSearchResponse.CreateSuccess(normalizedQuery, rawResponse, responseDoc.RootElement.Clone());
        }
        catch (JsonException ex)
        {
          return McpSearchResponse.CreateBackendError((int)response.StatusCode, normalizedQuery, rawResponse, $"ElasticSearch returned invalid JSON: {ex.Message}");
        }
      }
      catch (HttpRequestException ex)
      {
        return McpSearchResponse.CreateTransportError(normalizedQuery, $"ElasticSearch request failed: {ex.Message}");
      }
      catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
      {
        return McpSearchResponse.CreateTransportError(normalizedQuery, $"ElasticSearch request timed out: {ex.Message}");
      }
      catch (TaskCanceledException)
      {
        return McpSearchResponse.CreateTransportError(normalizedQuery, "ElasticSearch request was canceled.");
      }
    }
  }

  public sealed record McpSearchResponse
  {
    public bool Success { get; init; }

    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public string Query { get; init; } = string.Empty;

    public string RawResponse { get; init; } = string.Empty;

    public JsonElement? Result { get; init; }

    public static McpSearchResponse CreateSuccess(string query, string rawResponse, JsonElement result)
      => new()
      {
        Success = true,
        StatusCode = (int)HttpStatusCode.OK,
        Message = "Query executed successfully.",
        Query = query,
        RawResponse = rawResponse,
        Result = result
      };

    public static McpSearchResponse CreateValidationError(string message)
      => new()
      {
        Success = false,
        StatusCode = (int)HttpStatusCode.BadRequest,
        Message = message
      };

    public static McpSearchResponse CreateBackendError(int statusCode, string query, string rawResponse, string? message = null)
      => new()
      {
        Success = false,
        StatusCode = statusCode,
        Message = message ?? "ElasticSearch returned an error response.",
        Query = query,
        RawResponse = rawResponse
      };

    public static McpSearchResponse CreateTransportError(string query, string message)
      => new()
      {
        Success = false,
        StatusCode = (int)HttpStatusCode.ServiceUnavailable,
        Message = message,
        Query = query
      };
  }

  public sealed record McpSourceListResponse
  {
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public string[] Sources { get; init; } = Array.Empty<string>();

    public int Count => Sources.Length;

    public static McpSourceListResponse CreateSuccess(string[] sources)
      => new()
      {
        Success = true,
        Message = "Source list loaded successfully.",
        Sources = sources
      };

    public static McpSourceListResponse CreateFailure(string message)
      => new()
      {
        Success = false,
        Message = message
      };
  }
}
