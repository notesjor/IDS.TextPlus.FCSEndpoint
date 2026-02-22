using System.Text;
using IDS.TextPlus.FCSEndpoint.Helper;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint.Version.Abstract;

public abstract class AbstractVersion
{
  /// <summary>
  ///   Maximum records per response
  /// </summary>
  protected int _maxRecords = 1000;

  /// <summary>
  ///   MIME-Type for response
  /// </summary>
  protected string _mime = "application/xml;charset=utf-8";

  /// <summary>
  ///   This function is called for every new HTTP-Request
  /// </summary>
  /// <param name="ctx">Current HttpContext</param>
  /// <param name="data">Dictionary (key -> lowerCase) of GET-Params</param>
  public abstract void ProcessRequest(HttpContext ctx, ref Dictionary<string, string> data);

  /// <summary>
  /// </summary>
  /// <param name="ctx">Current HttpContext</param>
  /// <param name="data">Dictionary (key -> lowerCase) of GET-Params</param>
  /// <param name="name">name (key) of Parameter (to check)</param>
  /// <param name="nameSpec">name of Parameter in the FCs-Specs</param>
  /// <param name="defaultValue">default Value of the Parameter (if not set)</param>
  /// <param name="minValue">minimum Value - if Value is lower the client get an error message</param>
  /// <param name="returnValue">Return value (use out var to get this value) if correct</param>
  /// <param name="template">Template string for Error-Message</param>
  /// <returns>Passes the parameter all tests?</returns>
  protected bool GetUrlParameterNumber(HttpContext ctx, ref Dictionary<string, string> data, string name,
    string nameSpec, int defaultValue, int minValue, out int returnValue, string template)
  {
    returnValue = defaultValue;
    if (data.ContainsKey(name))
    {
      if (!int.TryParse(data[name], out returnValue))
      {
        ctx.Response.Send(template.Replace("{{name}}", nameSpec).Replace("{{message}}", "Invalid number format."),
          _mime);
        return true;
      }

      if (returnValue < minValue)
      {
        ctx.Response.Send(
          template.Replace("{{name}}", nameSpec).Replace("{{message}}", $"Value is less than {minValue}."), _mime);
        return true;
      }
    }

    return false;
  }

  /// <summary>
  ///   Checks for different URL-Parameters
  /// </summary>
  /// <param name="ctx">Current HttpContext</param>
  /// <param name="data">Dictionary (key -> lowerCase) of GET-Params</param>
  /// <param name="name">name (key) of Parameter (to check)</param>
  /// <param name="nameSpec">name of Parameter in the FCs-Specs</param>
  /// <param name="defaultValue">default Value of the Parameter (if not set)</param>
  /// <param name="minValue">minimum Value - if Value is lower the client get an error message</param>
  /// <param name="maxValue">maximum Value - if Value is higher the client get an error message</param>
  /// <param name="returnValue">Return value (use out var to get this value) if correct</param>
  /// <param name="template">Template string for Error-Message</param>
  /// <returns>Passes the parameter all tests?</returns>
  protected bool GetUrlParameterNumber(HttpContext ctx, ref Dictionary<string, string> data, string name,
    string nameSpec, int defaultValue, int minValue, int maxValue, out int returnValue, string template)
  {
    returnValue = defaultValue;
    if (data.ContainsKey(name))
    {
      if (!int.TryParse(data[name], out returnValue))
      {
        ctx.Response.Send(template.Replace("{{name}}", nameSpec).Replace("{{message}}", "Invalid number format."),
          _mime);
        return true;
      }

      if (returnValue < minValue)
      {
        ctx.Response.Send(
          template.Replace("{{name}}", nameSpec).Replace("{{message}}", $"Value is less than {minValue}."), _mime);
        return true;
      }

      if (returnValue > maxValue)
      {
        ctx.Response.Send(
          template.Replace("{{name}}", nameSpec).Replace("{{message}}", $"Value is greater than {maxValue}."), _mime);
        return true;
      }
    }

    return false;
  }

  protected string BuildEndpointDescription(string version)
  {
    var doc = File.ReadAllText($"Snippets/{version}/{version}EndpointDescription.xml", Encoding.UTF8);
    var ent = File.ReadAllText($"Snippets/{version}/{version}EndpointDescriptionResource.xml", Encoding.UTF8);
    var lan = File.ReadAllText($"Snippets/{version}/{version}EndpointDescriptionResourceLanguage.xml", Encoding.UTF8);

    var catalog = SearchResourceHelper.Catalog;

    var resources = string.Join("\r\n", catalog.Values.Select(x => ent.Replace("{{pid}}", x.Pid)
      .Replace("{{title_deu}}", x.Info["deu"]["Title"])
      .Replace("{{title_eng}}", x.Info["eng"]["Title"])
      .Replace("{{description_deu}}", x.Info["deu"]["Description"])
      .Replace("{{description_eng}}", x.Info["eng"]["Description"])
      .Replace("{{institution_deu}}", x.Info["deu"]["Institution"])
      .Replace("{{institution_eng}}", x.Info["eng"]["Institution"])
      .Replace("{{landingpage}}", x.Url)
      .Replace("{{languages}}", string.Join("\r\n", x.Languages.Select(y => lan.Replace("{{langauge}}", y))))
      .Replace("{{dataviews}}", x.DataViews)
      .Replace("{{lexfields}}", x.LexFields)));

    return doc.Replace("{{resources}}", resources);
  }
}