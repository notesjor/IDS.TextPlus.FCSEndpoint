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
    public abstract bool DefaultRoute(HttpContext ctx);
    public abstract bool Resources(HttpContext ctx);
    public abstract bool EndpointDescription(HttpContext ctx);
    public abstract bool DataviewHits(HttpContext ctx);
    public abstract bool DataviewAdvanced(HttpContext ctx);
  }
}
