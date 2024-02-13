using IDS.TextPlus.FCSEndpoint.Model;
using IDS.TextPlus.FCSEndpoint.Version.Abstract;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint.Version
{
  public class Version12 : AbstractVersion
  {
    private string DefaultRouteResponse;
    private string EmptyResult;
    private string EndpointDescriptionResponse;
    private string Error_OutOfRange;
    private string Error_QueryParser;
    private string Template_Error_RecordXmlEscaping;
    private string Template_Error_Number;
    private string Template_Error_ScanNumber;
    private string Template_Response_01;
    private string Template_Response_02;
    private string Template_Response_03;
    private string Template_Response_04;
    private string Template_Response_05;

    public Version12()
    {
      DefaultRouteResponse = System.IO.File.ReadAllText("Snippets/12/12DefaultRoute.xml", Encoding.UTF8).Replace("{{max}}", _maxRecords.ToString());
      EndpointDescriptionResponse = DefaultRouteResponse;
      Error_QueryParser = System.IO.File.ReadAllText("Snippets/12/12Error_QueryParser.xml", Encoding.UTF8);
      Template_Error_RecordXmlEscaping = System.IO.File.ReadAllText("Snippets/12/12Template_Error_recordPacking.xml", Encoding.UTF8);
      Template_Error_Number = System.IO.File.ReadAllText("Snippets/12/12Template_Error_Number.xml", Encoding.UTF8);
      EmptyResult = System.IO.File.ReadAllText("Snippets/12/12EmptyResult.xml", Encoding.UTF8);
      Error_OutOfRange = System.IO.File.ReadAllText("Snippets/12/12Error_OutOfRange.xml", Encoding.UTF8);
      Template_Error_ScanNumber = System.IO.File.ReadAllText("Snippets/12/12Template_Error_Scan_Number.xml", Encoding.UTF8);
      Template_Response_01 = System.IO.File.ReadAllText("Snippets/12/12Template_Response_01.xml", Encoding.UTF8);
      Template_Response_02 = System.IO.File.ReadAllText("Snippets/12/12Template_Response_02.xml", Encoding.UTF8);
      Template_Response_03 = System.IO.File.ReadAllText("Snippets/12/12Template_Response_03.xml", Encoding.UTF8);
      Template_Response_04 = System.IO.File.ReadAllText("Snippets/12/12Template_Response_04.xml", Encoding.UTF8);
      Template_Response_05 = System.IO.File.ReadAllText("Snippets/12/12Template_Response_05.xml", Encoding.UTF8);      
    }

    public override void ProcessRequest(HttpContext ctx, ref Dictionary<string, string> data)
    {
      if (data.ContainsKey("x-fcs-endpoint-description"))
      {
        ctx.Response.Send(EndpointDescriptionResponse, _mime);
        return;
      }

      if (data.ContainsKey("recordpacking") && data["recordpacking"].ToLower() != "xml")
      {
        ctx.Response.Send(Template_Error_RecordXmlEscaping.Replace("{{format}}", data["recordpacking"]));
        return;
      }

      GetUrlParameterNumber(ctx, ref data, "startrecord", "startRecord", 1, 1, out var start, Template_Error_Number);
      GetUrlParameterNumber(ctx, ref data, "responseposition", "responsePosition", start, 1, out start, Template_Error_ScanNumber); // startRecord = responsePosition
      GetUrlParameterNumber(ctx, ref data, "maximumrecords", "maximumRecords", 10, 0, _maxRecords, out var maximum, Template_Error_Number);
      GetUrlParameterNumber(ctx, ref data, "maximumterms", "maximumTerms", maximum, 0, _maxRecords, out maximum, Template_Error_ScanNumber); // maximumRecords = maximumTerms

      if (data.ContainsKey("query"))
      {
        ExecuteQuery(ctx, data["query"], start, maximum);
        return;
      }
      if(data.ContainsKey("operation") && data["operation"] == "scan")
      {
        ExecuteQuery(ctx, "", start, maximum);
        return;
      }
      
      ctx.Response.Send(DefaultRouteResponse, _mime);
    }

    private void ExecuteQuery(HttpContext ctx, string query, int start, int maximum)
    {
      if (query.Contains("="))
      {
        ctx.Response.Send(Error_QueryParser, _mime);
        return;
      }

      SearchResponse result = SendSearchRequest(query, start, maximum);

      if (result?.Hits == null || result.Hits.Count == 0)
      {
        if (start > 1 && start > result?.EstimatedTotalHits)
        {
          ctx.Response.Send(Error_OutOfRange, _mime);
          return;
        }

        ctx.Response.Send(EmptyResult.Replace("{{query}}", query), _mime);
        return;
      }

      ctx.Response.SendChunk(Template_Response_01, mimeType: _mime);
      ctx.Response.SendChunk(result.EstimatedTotalHits.ToString());
      ctx.Response.SendChunk(Template_Response_02);

      for (int i = 0; i < result.Hits.Count; i++)
        ctx.Response.SendChunk(Template_Response_03.Replace("{{id}}", result.Hits[i].id).Replace("{{url}}", result.Hits[i].url).Replace("{{hit}}", result.Hits[i].text).Replace("{{p}}", (result.Offset + i).ToString()));

      ctx.Response.SendChunk(Template_Response_04);
      ctx.Response.SendFinalChunk(Template_Response_05.Replace("{{query}}", query).Replace("{{start}}", (result.Offset + 1).ToString()).Replace("{{offset}}", start.ToString()).Replace("{{max}}", result.EstimatedTotalHits.ToString()));
    }
  }
}
