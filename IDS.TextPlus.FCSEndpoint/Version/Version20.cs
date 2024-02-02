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
    private string DefaultRouteResponse;
    private string EndpointDescriptionResponse;
    private string Template_Error_RecordXmlEscaping;
    private string Template_Error_Number;
    private string Template_Error_ScanNumber;

    public Version20()
    {
      DefaultRouteResponse = System.IO.File.ReadAllText("Snippets/20DefaultRoute.xml", Encoding.UTF8);
      EndpointDescriptionResponse = System.IO.File.ReadAllText("Snippets/20EndpointDescription.xml", Encoding.UTF8);
      Template_Error_RecordXmlEscaping = System.IO.File.ReadAllText("Snippets/20Template_Error_recordXMLEscaping.xml", Encoding.UTF8);
      Template_Error_Number = System.IO.File.ReadAllText("Snippets/20Template_Error_Number.xml", Encoding.UTF8);
      Template_Error_ScanNumber = System.IO.File.ReadAllText("Snippets/20Template_Error_Scan_Number.xml", Encoding.UTF8);
    }

    public override void ProcessRequest(HttpContext ctx)
    {
      var data = ctx.GetData();
      if(data.ContainsKey("x-fcs-endpoint-description") )
      {
        ctx.Response.Send(EndpointDescriptionResponse, _mime);
        return;
      }

      GetUrlParameterNumber(ctx, ref data, "startrecord", "startRecord", 1, 1, out var start, Template_Error_Number);
      GetUrlParameterNumber(ctx, ref data, "responseposition", "responsePosition", 1, 1, out start, Template_Error_ScanNumber); // startRecord = responsePosition
      GetUrlParameterNumber(ctx, ref data, "maximumrecords", "maximumRecords", 10, 0, 250, out var maximum, Template_Error_Number);
      GetUrlParameterNumber(ctx, ref data, "maximumterms", "maximumTerms", 10, 0, 250, out maximum, Template_Error_ScanNumber); // maximumRecords = maximumTerms

      var recordXMLEscaping = ""; // stupid parameter, but it's in the requirements
      if (data.ContainsKey("recordxmlescaping"))
      {
        var escaping = data["recordxmlescaping"];
        switch (escaping)
        {
          case "none":
            recordXMLEscaping = "";
            break;
          case "xml":
            recordXMLEscaping = "<sruResponse:recordXMLEscaping>xml</sruResponse:recordXMLEscaping>";
            break;
          default:
            ctx.Response.Send(Template_Error_RecordXmlEscaping.Replace("{{template}}", escaping), _mime);
            return;
        }
      }

      ctx.Response.Send(DefaultRouteResponse, _mime);
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
