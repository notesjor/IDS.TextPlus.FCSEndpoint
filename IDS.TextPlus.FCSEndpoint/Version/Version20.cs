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
  public class Version20 : AbstractVersion
  {
    private string _mime = "application/xml;charset=utf-8";
    private int _maxRecords = 1000;

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

    public Version20()
    {
      DefaultRouteResponse = System.IO.File.ReadAllText("Snippets/20DefaultRoute.xml", Encoding.UTF8).Replace("{{max}}", _maxRecords.ToString());
      EmptyResult = System.IO.File.ReadAllText("Snippets/20EmptyResult.xml", Encoding.UTF8);
      EndpointDescriptionResponse = System.IO.File.ReadAllText("Snippets/20EndpointDescription.xml", Encoding.UTF8);
      Error_OutOfRange = System.IO.File.ReadAllText("Snippets/20Error_OutOfRange.xml", Encoding.UTF8);
      Error_QueryParser = System.IO.File.ReadAllText("Snippets/20Error_QueryParser.xml", Encoding.UTF8);
      Template_Error_RecordXmlEscaping = System.IO.File.ReadAllText("Snippets/20Template_Error_recordXMLEscaping.xml", Encoding.UTF8);
      Template_Error_Number = System.IO.File.ReadAllText("Snippets/20Template_Error_Number.xml", Encoding.UTF8);
      Template_Error_ScanNumber = System.IO.File.ReadAllText("Snippets/20Template_Error_Scan_Number.xml", Encoding.UTF8);

      Template_Response_01 = System.IO.File.ReadAllText("Snippets/20Template_Response_01.xml", Encoding.UTF8);
      Template_Response_02 = System.IO.File.ReadAllText("Snippets/20Template_Response_02.xml", Encoding.UTF8);
      Template_Response_03 = System.IO.File.ReadAllText("Snippets/20Template_Response_03.xml", Encoding.UTF8);
      Template_Response_04 = System.IO.File.ReadAllText("Snippets/20Template_Response_04.xml", Encoding.UTF8);
      Template_Response_05 = System.IO.File.ReadAllText("Snippets/20Template_Response_05.xml", Encoding.UTF8);
    }

    public override void ProcessRequest(HttpContext ctx)
    {
      var data = ctx.GetData();
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
      GetUrlParameterNumber(ctx, ref data, "responseposition", "responsePosition", start, 1, out start, Template_Error_ScanNumber); // startRecord = responsePosition
      GetUrlParameterNumber(ctx, ref data, "maximumrecords", "maximumRecords", 10, 0, _maxRecords, out var maximum, Template_Error_Number);
      GetUrlParameterNumber(ctx, ref data, "maximumterms", "maximumTerms", maximum, 0, _maxRecords, out maximum, Template_Error_ScanNumber); // maximumRecords = maximumTerms

      if (data.ContainsKey("query"))
        ExecuteQuery(ctx, data["query"], start, maximum);
      else
        ctx.Response.Send(DefaultRouteResponse, _mime);
    }

    private void ExecuteQuery(HttpContext ctx, string query, int start, int maximum)
    {
      if (query.Contains("="))
      {
        ctx.Response.Send(Error_QueryParser, _mime);
        return;
      }

      var client = new RestClient(new RestClientOptions("http://lexik08.ids-mannheim.de:7700") { MaxTimeout = 5000 });
      var request = new RestRequest("/indexes/fcs/search", Method.Post);
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Authorization", "Bearer 8jRAqq_GbtjdjveIOCxIlnztXjwFbcaMYp-e50HtbrQ");
      request.AddStringBody(JsonConvert.SerializeObject(new SearchRequest
      {
        q = query,
        limit = maximum,
        offset = start - 1
      }), ContentType.Json);
      var response = client.ExecuteAsync(request);
      response.Wait();

      var result = JsonConvert.DeserializeObject<SearchResponse>(response.Result.Content);      

      if(result.Hits.Count == 0)
      {
        if (start > 1 && start > result.EstimatedTotalHits)
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
      foreach (var hit in result.Hits)
        ctx.Response.SendChunk(Template_Response_03.Replace("{{id}}", hit.id).Replace("{{url}}", hit.url).Replace("{{hit}}", hit.text));
      ctx.Response.SendChunk(Template_Response_04);
      ctx.Response.SendFinalChunk(Template_Response_05.Replace("{{query}}", query).Replace("{{start}}", (result.Offset + 1).ToString()));
    }

    private bool GetUrlParameterNumber(HttpContext ctx, ref Dictionary<string, string> data, string name, string nameSpec, int defaultValue, int minValue, out int returnValue, string template)
    {
      returnValue = defaultValue;
      if (data.ContainsKey(name))
      {
        if (!int.TryParse(data[name], out returnValue))
        {
          ctx.Response.Send(template.Replace("{{name}}", "responseposition").Replace("{{message}}", "Invalid number format."), _mime);
          return true;
        }
        if (returnValue < minValue)
        {
          ctx.Response.Send(template.Replace("{{name}}", "responseposition").Replace("{{message}}", $"Value is less than {minValue}."), _mime);
          return true;
        }
      }

      return false;
    }

    private bool GetUrlParameterNumber(HttpContext ctx, ref Dictionary<string, string> data, string name, string nameSpec, int defaultValue, int minValue, int maxValue, out int returnValue, string template)
    {
      returnValue = defaultValue;
      if (data.ContainsKey(name))
      {
        if (!int.TryParse(data[name], out returnValue))
        {
          ctx.Response.Send(template.Replace("{{name}}", "responseposition").Replace("{{message}}", "Invalid number format."), _mime);
          return true;
        }
        if (returnValue < minValue)
        {
          ctx.Response.Send(template.Replace("{{name}}", "responseposition").Replace("{{message}}", $"Value is less than {minValue}."), _mime);
          return true;
        }
        if (returnValue > maxValue)
        {
          ctx.Response.Send(template.Replace("{{name}}", "responseposition").Replace("{{message}}", $"Value is greater than {maxValue}."), _mime);
          return true;
        }
      }

      return false;
    }
  }
}
