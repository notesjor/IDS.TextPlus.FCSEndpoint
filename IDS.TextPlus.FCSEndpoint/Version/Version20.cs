using System.Net;
using System.Text;
using IDS.TextPlus.FCSEndpoint.Helper;
using IDS.TextPlus.FCSEndpoint.Indexer.Model;
using IDS.TextPlus.FCSEndpoint.Model;
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

    if (data.ContainsKey("query"))
      ExecuteQuery(ctx, data["query"], start, maximum, context, provideDataView);
    else
      ctx.Response.Send(DefaultRouteResponse, _mime);
  }

  private void PrintDebug(Dictionary<string, string> data)
  {
    Console.WriteLine(string.Join("; ", data.Select(x => $"{x.Key}={x.Value}")));
  }

  private void ExecuteQuery(HttpContext ctx, string query, int start, int maximum, string? context, bool provideDataView)
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

      var stb = new StringBuilder();

      stb.Append(Template_Response_01);
      stb.Append(result.EstimatedTotalHits.ToString());
      stb.Append(Template_Response_02);

      var dict = SearchResourceHelper.KeyToPid;

      for (var i = 0; i < result.Hits.Length; i++)
        stb.Append(Template_Response_03.Replace("{{res_pid}}", dict[result.Hits[i].Source])
          .Replace("{{url}}", result.Hits[i].Url)
          .Replace("{{hit}}", result.Hits[i].Formatted.Text)
          .Replace("{{p}}", (result.Offset + i).ToString())
          .Replace("{{lex_dataview}}", provideDataView ? LexDataView(result.Hits[i]) : ""));

      stb.Append(Template_Response_04);
      stb.Append(Template_Response_05.Replace("{{query}}", query)
        .Replace("{{start}}", (result.Offset + 1).ToString())
        .Replace("{{offset}}", start.ToString())
        .Replace("{{max}}", result.EstimatedTotalHits.ToString())
        .Replace("{{next}}", (start + maximum).ToString()));

      ctx.Response.Send(stb.ToString(), _mime);
    }
    catch (TypeLoadException)
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

  private string? LexDataView(SearchResponseContainer resultHit)
  {
    var stb = new StringBuilder(Template_Response_03_ext);
    stb.Replace("{{lang}}", string.IsNullOrWhiteSpace(resultHit.Lang) ? "deu" : resultHit.Lang);
    stb.Replace("{{url}}", resultHit.Url);
    stb.Replace("{{lemma}}", resultHit.Lemma);
    stb.Replace("{{id}}", resultHit.OId);

    var fields = new string[]{
      AddFields(resultHit.PosFull, "pos"),
      AddFields(resultHit.NumberFull, "number"),
      AddFields(resultHit.GenderFull, "gender"),
      AddFields(resultHit.Link),
      AddFields(resultHit.Citation),
      resultHit.Segmentation == null
        ? string.Empty
        : $"<lex:Field type=\"segmentation\">\r\n  <lex:Value>{resultHit.Segmentation}</lex:Value>\r\n</lex:Field>\r\n",
      resultHit.Def == null
        ? string.Empty
        : $"<lex:Field type=\"definition\">\r\n  <lex:Value>{resultHit.Def}</lex:Value>\r\n</lex:Field>\r\n",
    };

    stb.Replace("{{extra_fields}}", string.Join("\r\n", fields));

    return stb.ToString();
  }

  private string AddFields(IList<SimpleValue>? values, string type)
  {
    if (values == null || values.Count == 0)
      return string.Empty;

    var stb = new StringBuilder($"<lex:Field type=\"{type}\">\r\n");
    foreach (var x in values)
      if (string.IsNullOrWhiteSpace(x.Schema))
        stb.Append($"  <lex:Value>{x.Value}</lex:Value>\r\n");
      else
        stb.Append($"  <lex:Value vocabValueRef=\"{x.Schema}\">{x.Value}</lex:Value>\r\n");
    stb.Append("</lex:Field>\r\n");

    return stb.ToString();
  }

  private string AddFields(IList<Link>? values)
  {
    if (values == null || values.Count == 0)
      return string.Empty;

    var types = values.Select(x => x.Type).Distinct().ToArray();

    var stb = new StringBuilder();
    foreach (var type in types)
    {
      stb.Append($"<lex:Field type=\"{type}\">\r\n");
      foreach (var x in values)
        stb.Append($"  <lex:Value ref=\"{x.Target}\">{x.Value}</lex:Value>\r\n");
      stb.Append("</lex:Field>\r\n");
    }

    return stb.ToString();
  }

  private string AddFields(IList<Citation>? values)
  {
    if (values == null || values.Count == 0)
      return string.Empty;

    var stb = new StringBuilder("<lex:Field type=\"citation\">\r\n");
    foreach (var x in values)
      //stb.Append($"  <lex:Value type=\"example\" source=\"{x.Source.Replace("\"", "'")}\">{x.Example}</lex:Value>\r\n");
      stb.Append($"  <lex:Value type=\"example\">{x.Example}</lex:Value>\r\n");
    stb.Append("</lex:Field>\r\n");

    return stb.ToString();
  }
}