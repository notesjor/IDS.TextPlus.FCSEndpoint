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
    private string DefaultRouteResponse = "";
    private string EndpointDescriptionResponse = "";

    public Version20()
    {
      DefaultRouteResponse = System.IO.File.ReadAllText("Snippets/20DefaultRoute.xml", Encoding.UTF8);
      EndpointDescriptionResponse = System.IO.File.ReadAllText("Snippets/20EndpointDescription.xml", Encoding.UTF8);
    }

    public override bool DataviewAdvanced(HttpContext ctx)
    {
      throw new NotImplementedException();
    }

    public override bool DataviewHits(HttpContext ctx)
    {
      throw new NotImplementedException();
    }

    public override bool DefaultRoute(HttpContext ctx)
    {
      var data = ctx.GetData();
      if(data.ContainsKey("x-fcs-endpoint-description") )
      {
        ctx.Response.Send(EndpointDescriptionResponse);
        return true;
      }

      ctx.Response.Send(DefaultRouteResponse);
      return true;
    }

    public override bool EndpointDescription(HttpContext ctx)
    {
      ctx.Response.Send(EndpointDescriptionResponse);
      return true;
    }

    public override bool Resources(HttpContext ctx)
    {
      throw new NotImplementedException();
    }
  }
}
