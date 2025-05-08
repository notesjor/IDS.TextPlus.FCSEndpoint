using System.Net;
using System.Text;
using IDS.TextPlus.FCSEndpoint.Helper;
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
    Template_Response_04 = File.ReadAllText("Snippets/20/20Template_Response_04.xml", Encoding.UTF8);
    Template_Response_05 = File.ReadAllText("Snippets/20/20Template_Response_05.xml", Encoding.UTF8);
  }

  public override void ProcessRequest(HttpContext ctx, ref Dictionary<string, string> data)
  {
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

    if (data.ContainsKey("query"))
      ExecuteQuery(ctx, data["query"], start, maximum);
    else
      ctx.Response.Send(DefaultRouteResponse, _mime);
  }

  private void ExecuteQuery(HttpContext ctx, string query, int start, int maximum)
  {
    try
    {
      // FCS specs require an error message if the query is empty - but the regular CQL parser does not throw an exception in this case.
      if (query.EndsWith("="))
      {
        ctx.Response.Send(Error_QuerySyntax, _mime);
        return;
      }

      var result = Search.Send(query, start, maximum);

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

      var stb = new StringBuilder();

      stb.Append(Template_Response_01);
      stb.Append(result.EstimatedTotalHits.ToString());
      stb.Append(Template_Response_02);

      for (var i = 0; i < result.Hits.Length; i++)
        stb.Append(Template_Response_03.Replace("{{id}}", result.Hits[i].Formatted.Id)
          .Replace("{{url}}", result.Hits[i].Formatted.Url).Replace("{{hit}}", result.Hits[i].Formatted.Text)
          .Replace("{{p}}", (result.Offset + i).ToString()));

      stb.Append(Template_Response_04);
      stb.Append(Template_Response_05.Replace("{{query}}", query).Replace("{{start}}", (result.Offset + 1).ToString())
        .Replace("{{offset}}", start.ToString()).Replace("{{max}}", result.EstimatedTotalHits.ToString()));

      ctx.Response.Send(stb.ToString(), _mime);
    }
    catch (TypeLoadException)
    {
      ctx.Response.Send(Error_QueryParser);
    }
    catch
    {
      ctx.Response.Send(HttpStatusCode.InternalServerError);
    }
  }
}