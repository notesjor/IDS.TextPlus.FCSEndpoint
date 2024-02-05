using IDS.TextPlus.FCSEndpoint.Version.Abstract;
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
    private string EndpointDescriptionResponse;
    private string Template_Error_RecordXmlEscaping;
    private string Template_Error_Number;
    private string Template_Error_ScanNumber;

    public Version20()
    {
      DefaultRouteResponse = System.IO.File.ReadAllText("Snippets/20DefaultRoute.xml", Encoding.UTF8).Replace("{{max}}", _maxRecords.ToString());
      EndpointDescriptionResponse = System.IO.File.ReadAllText("Snippets/20EndpointDescription.xml", Encoding.UTF8);
      Template_Error_RecordXmlEscaping = System.IO.File.ReadAllText("Snippets/20Template_Error_recordXMLEscaping.xml", Encoding.UTF8);
      Template_Error_Number = System.IO.File.ReadAllText("Snippets/20Template_Error_Number.xml", Encoding.UTF8);
      Template_Error_ScanNumber = System.IO.File.ReadAllText("Snippets/20Template_Error_Scan_Number.xml", Encoding.UTF8);
    }

    public override void ProcessRequest(HttpContext ctx)
    {
      var data = ctx.GetData();
      if (data.ContainsKey("x-fcs-endpoint-description"))
      {
        ctx.Response.Send(EndpointDescriptionResponse, _mime);
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
      Console.WriteLine(query);
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
