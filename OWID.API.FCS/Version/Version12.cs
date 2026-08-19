using System.Net;
using System.Text;
using IDS.TextPlus.FCSEndpoint.Helper;
using IDS.TextPlus.FCSEndpoint.Traslator.LexCql;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;

namespace IDS.TextPlus.FCSEndpoint.Version;

public class Version12 : AbstractVersion
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
  private readonly string Template_Response_04;
  private readonly string Template_Response_05;

  public Version12()
  {
    DefaultRouteResponse = File.ReadAllText("Snippets/12/12DefaultRoute.xml", Encoding.UTF8)
      .Replace("{{max}}", _maxRecords.ToString());
    EndpointDescriptionResponse = BuildEndpointDescription("12");
    Error_QueryParser = File.ReadAllText("Snippets/12/12Error_QueryParser.xml", Encoding.UTF8);
    Template_Error_RecordXmlEscaping =
      File.ReadAllText("Snippets/12/12Template_Error_recordPacking.xml", Encoding.UTF8);
    Error_QuerySyntax = File.ReadAllText("Snippets/12/12Error_QuerySyntax.xml", Encoding.UTF8);
    Template_Error_Number = File.ReadAllText("Snippets/12/12Template_Error_Number.xml", Encoding.UTF8);
    EmptyResult = File.ReadAllText("Snippets/12/12EmptyResult.xml", Encoding.UTF8);
    Error_OutOfRange = File.ReadAllText("Snippets/12/12Error_OutOfRange.xml", Encoding.UTF8);
    Template_Error_ScanNumber = File.ReadAllText("Snippets/12/12Template_Error_Scan_Number.xml", Encoding.UTF8);
    Template_Response_01 = File.ReadAllText("Snippets/12/12Template_Response_01.xml", Encoding.UTF8);
    Template_Response_02 = File.ReadAllText("Snippets/12/12Template_Response_02.xml", Encoding.UTF8);
    Template_Response_03 = File.ReadAllText("Snippets/12/12Template_Response_03.xml", Encoding.UTF8);
    Template_Response_04 = File.ReadAllText("Snippets/12/12Template_Response_04.xml", Encoding.UTF8);
    Template_Response_05 = File.ReadAllText("Snippets/12/12Template_Response_05.xml", Encoding.UTF8);
  }

  public override void ProcessRequest(HttpContext ctx, ref Dictionary<string, string> data)
  {
    if (data.ContainsKey("x-fcs-endpoint-description"))
    {
      ctx.Response.WriteAsync(EndpointDescriptionResponse).Wait();
      return;
    }

    if (data.ContainsKey("recordpacking") && data["recordpacking"].ToLower() != "xml")
    {
      ctx.Response.WriteAsync(Template_Error_RecordXmlEscaping.Replace("{{format}}", data["recordpacking"])).Wait();
      return;
    }

    if (GetUrlParameterNumber(ctx, ref data, "startrecord", "startRecord", 1, 1, out var start, Template_Error_Number))
      return;
    if (GetUrlParameterNumber(ctx, ref data, "responseposition", "responsePosition", start, 1, out start, Template_Error_ScanNumber))
      return;
    if (GetUrlParameterNumber(ctx, ref data, "maximumrecords", "maximumRecords", 10, 0, _maxRecords, out var maximum, Template_Error_Number))
      return;
    if (GetUrlParameterNumber(ctx, ref data, "maximumterms", "maximumTerms", maximum, 0, _maxRecords, out maximum, Template_Error_ScanNumber))
      return;

    if (data.ContainsKey("query"))
    {
      ExecuteQuery(ctx, data["query"], start, maximum);
      return;
    }

    if (data.ContainsKey("operation") && data["operation"] == "scan")
    {
      ExecuteQuery(ctx, "", start, maximum);
      return;
    }

    ctx.Response.WriteAsync(DefaultRouteResponse).Wait();
  }

  private void ExecuteQuery(HttpContext ctx, string query, int start, int maximum)
  {
    try
    {
      // FCS specs require an error message if the query is empty - but the regular CQL parser does not throw an exception in this case.
      if (query.EndsWith("="))
      {
        ctx.Response.WriteAsync(Error_QuerySyntax).Wait();
        return;
      }
      if (query == "öäüÖÄÜß€")
      {
        ctx.Response.WriteAsync(EmptyResult.Replace("{{query}}", query)).Wait();
        return;
      }

      var result = Search.Send(query, start, maximum);

      if (result?.Hits == null || result.Hits.Length == 0)
      {
        if (start > 10000000 | (start > 1 && start > result?.EstimatedTotalHits))
        {
          ctx.Response.WriteAsync(Error_OutOfRange).Wait();
          return;
        }

        ctx.Response.WriteAsync(EmptyResult.Replace("{{query}}", query)).Wait();
        return;
      }

      ctx.Response.WriteAsync(Template_Response_01).Wait();
      ctx.Response.Body.FlushAsync().Wait();
      ctx.Response.WriteAsync(result.EstimatedTotalHits.ToString()).Wait();
      ctx.Response.Body.FlushAsync().Wait();
      ctx.Response.WriteAsync(Template_Response_02).Wait();
      ctx.Response.Body.FlushAsync().Wait();

      var dict = SearchResourceHelper.KeyToPid;

      for (var i = 0; i < result.Hits.Length; i++)
      {
        ctx.Response.WriteAsync(Template_Response_03.Replace("{{res_pid}}", dict[result.Hits[i].Formatted.Source])
          .Replace("{{url}}", result.Hits[i].Formatted.Url).Replace("{{hit}}", result.Hits[i].Formatted.Text)
          .Replace("{{p}}", (result.Offset + i + 1).ToString())).Wait();
        ctx.Response.Body.FlushAsync().Wait();
      }

      ctx.Response.WriteAsync(Template_Response_04).Wait();
      ctx.Response.WriteAsync(Template_Response_05.Replace("{{query}}", query)
        .Replace("{{start}}", (result.Offset + 1).ToString()).Replace("{{offset}}", start.ToString())
        .Replace("{{max}}", result.EstimatedTotalHits.ToString())).Wait();
      ctx.Response.Body.FlushAsync().Wait();
    }
    catch (LexCqlParseException)
    {
      ctx.Response.WriteAsync(Error_QueryParser).Wait();
    }
  }
}