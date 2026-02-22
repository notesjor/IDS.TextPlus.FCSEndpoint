using System.Net;
using System.Text;
using IDS.TextPlus.FCSEndpoint.Helper;
using IDS.TextPlus.FCSEndpoint.Model;
using IDS.TextPlus.FCSEndpoint.Traslator.LexCql;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint.Version;

public class Version20 : AbstractVersion
{
  private readonly string DefaultRouteResponse;
  private readonly string EmptyResult;
  private readonly string EndpointDescriptionResponse;
  private readonly string Error_OutOfRange;
  private readonly string Error_QueryParser;
  private readonly string Error_QuerySyntax;
  private readonly string Template_Error_Number;
  private readonly string Template_Error_RecordXmlEscaping;
  private readonly string Template_Error_ScanNumber;
  private readonly string Template_Response_01;
  private readonly string Template_Response_02;
  private readonly string Template_Response_03;
  private readonly string Template_Response_03_ext;
  private readonly string Template_Response_04;
  private readonly string Template_Response_05;

  public Version20()
  {
    DefaultRouteResponse = File.ReadAllText("Snippets/20/20DefaultRoute.xml", Encoding.UTF8)
      .Replace("{{max}}", _maxRecords.ToString());
    EmptyResult = File.ReadAllText("Snippets/20/20EmptyResult.xml", Encoding.UTF8);
    EndpointDescriptionResponse = BuildEndpointDescription("20");

    Error_OutOfRange = File.ReadAllText("Snippets/20/20Error_OutOfRange.xml", Encoding.UTF8);
    Error_QueryParser = File.ReadAllText("Snippets/20/20Error_QueryParser.xml", Encoding.UTF8);
    Error_QuerySyntax = File.ReadAllText("Snippets/20/20Error_QuerySyntax.xml", Encoding.UTF8);

    Template_Error_RecordXmlEscaping =
      File.ReadAllText("Snippets/20/20Template_Error_recordXMLEscaping.xml", Encoding.UTF8);
    Template_Error_Number = File.ReadAllText("Snippets/20/20Template_Error_Number.xml", Encoding.UTF8);
    Template_Error_ScanNumber = File.ReadAllText("Snippets/20/20Template_Error_Scan_Number.xml", Encoding.UTF8);

    Template_Response_01 = File.ReadAllText("Snippets/20/20Template_Response_01.xml", Encoding.UTF8);
    Template_Response_02 = File.ReadAllText("Snippets/20/20Template_Response_02.xml", Encoding.UTF8);
    Template_Response_03 = File.ReadAllText("Snippets/20/20Template_Response_03.xml", Encoding.UTF8);
    Template_Response_03_ext = File.ReadAllText("Snippets/20/20Template_Response_03_ext.xml", Encoding.UTF8);
    Template_Response_04 = File.ReadAllText("Snippets/20/20Template_Response_04.xml", Encoding.UTF8);
    Template_Response_05 = File.ReadAllText("Snippets/20/20Template_Response_05.xml", Encoding.UTF8);
  }

  public override void ProcessRequest(HttpContext ctx, ref Dictionary<string, string> data)
  {
#if DEBUG
    PrintDebug(data);
#endif

    if (data.ContainsKey("x-fcs-endpoint-description"))
    {
      ctx.Response.Send(EndpointDescriptionResponse, _mime);
      return;
    }

    if (data.ContainsKey("recordxmlescaping") && data["recordxmlescaping"].ToLower() != "xml")
    {
      ctx.Response.Send(Template_Error_RecordXmlEscaping.Replace("{{format}}", data["recordxmlescaping"]));
      return;
    }

    GetUrlParameterNumber(ctx, ref data, "startrecord", "startRecord", 1, 1, out var start, Template_Error_Number);
    GetUrlParameterNumber(ctx, ref data, "maximumrecords", "maximumRecords", 10, 0, _maxRecords, out var maximum,
      Template_Error_Number);
    // URL parameters are specified but meaningless. In version 1.2 they are used for the scan - which is not implemented in 2.0. But the parameters must be checked according to the specification.
    GetUrlParameterNumber(ctx, ref data, "responseposition", "responsePosition", start, 1, out start,
      Template_Error_ScanNumber); // startRecord = responsePosition
    GetUrlParameterNumber(ctx, ref data, "maximumterms", "maximumTerms", maximum, 0, _maxRecords, out maximum,
      Template_Error_ScanNumber); // maximumRecords = maximumTerms
    string context = null;
    if (data.ContainsKey("x-fcs-context"))
      context = data["x-fcs-context"];
    var provideDataView = (data.ContainsKey("querytype") && data["querytype"].ToLower() == "lex")
                          || (data.ContainsKey("x-fcs-dataviews") && data["x-fcs-dataviews"].ToLower() == "lex");
    HashSet<string> dataViewFilter = null;
    if (data.ContainsKey("x-fcs-lex-fields"))
      dataViewFilter = new HashSet<string>(data["x-fcs-lex-fields"].Split(',').Select(x => x.ToLower().Trim()));

    if (data.ContainsKey("query"))
      ExecuteQuery(ctx, data["query"], start, maximum, context, provideDataView, dataViewFilter);
    else
      ctx.Response.Send(DefaultRouteResponse, _mime);
  }

  private void PrintDebug(Dictionary<string, string> data)
  {
    Console.WriteLine(string.Join("; ", data.Select(x => $"{x.Key}={x.Value}")));
  }

  private void ExecuteQuery(HttpContext ctx, string query, int start, int maximum, string? context,
    bool provideDataView, HashSet<string> dataViewFilter)
  {
    try
    {
      // FCS specs require an error message if the query is empty - but the regular CQL parser does not throw an exception in this case.
      if (query.EndsWith("="))
      {
        ctx.Response.Send(Error_QuerySyntax, _mime);
        return;
      }

      var result = Search.Send(query, start, maximum, context);

      if (result?.Hits == null || result.Hits.Length == 0)
      {
        if (start > 1 && start > result?.EstimatedTotalHits)
        {
          ctx.Response.Send(Error_OutOfRange, _mime);
          return;
        }

        ctx.Response.Send(EmptyResult.Replace("{{query}}", query), _mime);
        return;
      }

      var totalResults = result.EstimatedTotalHits;
      var nextPage = start + maximum <= totalResults ? start + maximum : totalResults;

      ctx.Response.SendChunk(Template_Response_01, Encoding.UTF8, _mime);
      ctx.Response.SendChunk(result.EstimatedTotalHits.ToString());
      ctx.Response.SendChunk(Template_Response_02);

      var dict = SearchResourceHelper.KeyToPid;

      for (var i = 0; i < result.Hits.Length; i++)
      {
        var stb = new StringBuilder(Template_Response_03);
        stb.Replace("{{res_pid}}", dict[result.Hits[i].Source]);
        stb.Replace("{{url}}", result.Hits[i].Url);
        stb.Replace("{{hit}}", provideDataView ? LexDataViewHit(result.Hits[i]) : result.Hits[i].Formatted.Text);
        stb.Replace("{{p}}", (result.Offset + i + 1).ToString());
        stb.Replace("{{lex_dataview}}", provideDataView ? LexDataView(result.Hits[i], dataViewFilter) : "");
        ctx.Response.SendChunk(stb.ToString());
      }

      ctx.Response.SendChunk(Template_Response_04);
      var stbF = new StringBuilder(Template_Response_05);
      stbF.Replace("{{query}}", query);
      stbF.Replace("{{start}}", (result.Offset + 1).ToString());
      stbF.Replace("{{offset}}", start.ToString());
      stbF.Replace("{{max}}", result.EstimatedTotalHits.ToString());
      stbF.Replace("{{next}}", nextPage.ToString());
      ctx.Response.SendFinalChunk(stbF.ToString());
    }
    catch (LexCqlParseException)
    {
      ctx.Response.Send(Error_QueryParser);
    }
    catch (Exception ex)
    {
#if DEBUG
      Console.WriteLine(ex.Message);
      Console.WriteLine(ex.StackTrace);
#endif
      ctx.Response.Send(HttpStatusCode.InternalServerError);
    }
  }

  private string LexDataViewHit(SearchResponseContainer resultHit)
  {
    var stb = new StringBuilder();
    if (!string.IsNullOrWhiteSpace(resultHit.Lemma))
      stb.Append($"<hits:Hit kind=\"lex-lemma\">{resultHit.Lemma}</hits:Hit>");
    if (!string.IsNullOrWhiteSpace(resultHit.Segmentation))
      stb.Append($"<hits:Hit kind=\"lex-segmentation\">{resultHit.Segmentation}</hits:Hit>");
    if (resultHit.Synonym?.Length > 0)
      stb.Append($"<hits:Hit kind=\"lex-synonym\">{string.Join(", ", resultHit.Synonym)}</hits:Hit>");
    if (resultHit.Pos?.Length > 0)
      stb.Append($"<hits:Hit kind=\"lex-pos\">{string.Join(", ", resultHit.Pos)}</hits:Hit>");
    if (resultHit.Gender?.Length > 0)
      stb.Append($"<hits:Hit kind=\"lex-gender\">{string.Join(", ", resultHit.Gender)}</hits:Hit>");
    if (!string.IsNullOrWhiteSpace(resultHit.Definition))
      stb.Append($"<hits:Hit kind=\"lex-def\">{resultHit.Definition}</hits:Hit>");
    return stb.ToString();
  }

  private string? LexDataView(SearchResponseContainer resultHit, HashSet<string> dataViewFilter)
  {
    var stb = new StringBuilder(Template_Response_03_ext);
    stb.Replace("{{lang}}", string.IsNullOrWhiteSpace(resultHit.Lang) ? "deu" : resultHit.Lang);
    stb.Replace("{{url}}", resultHit.Url);
    stb.Replace("{{lemma}}", resultHit.Lemma);
    stb.Replace("{{id}}", resultHit.OId);

    var snippets = resultHit.FcsSnippets;
    //// NOTE: Der Aggregator unterstützte zeitweise keine citation-Snippets.
    //var snippets = resultHit.FcsSnippets.Where(x=> x.Key !/= /"citation")
    //  .ToDictionary(x => x.Key, x => x.Value);
    ////

    var fields = dataViewFilter == null
      ? snippets.Values
      : snippets.Where(x => dataViewFilter.Contains(x.Key)).Select(x => x.Value);

    stb.Replace("{{extra_fields}}", string.Join("", fields));

    return stb.ToString();
  }
}