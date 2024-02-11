using IDS.TextPlus.FCSEndpoint.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint.Version.Abstract
{
  public abstract class AbstractVersion
  {
    protected RestClient _client = new RestClient(new RestClientOptions("http://lexik08.ids-mannheim.de:7700") { MaxTimeout = 5000 });
    protected string _mime = "application/xml;charset=utf-8";
    protected int _maxRecords = 1000;

    public abstract void ProcessRequest(HttpContext ctx, ref Dictionary<string, string> data);

    protected SearchResponse SendSearchRequest(string query, int start, int maximum)
    {
      var request = new RestRequest("/indexes/fcs/search", Method.Post);
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Authorization", "Bearer 8jRAqq_GbtjdjveIOCxIlnztXjwFbcaMYp-e50HtbrQ");
      request.AddStringBody(JsonConvert.SerializeObject(new SearchRequest
      {
        q = query,
        limit = maximum,
        offset = start - 1
      }), ContentType.Json);
      var response = _client.ExecuteAsync(request);
      response.Wait();

      var result = JsonConvert.DeserializeObject<SearchResponse>(response.Result.Content);
      return result;
    }

    protected bool GetUrlParameterNumber(HttpContext ctx, ref Dictionary<string, string> data, string name, string nameSpec, int defaultValue, int minValue, out int returnValue, string template)
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

    protected bool GetUrlParameterNumber(HttpContext ctx, ref Dictionary<string, string> data, string name, string nameSpec, int defaultValue, int minValue, int maxValue, out int returnValue, string template)
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
