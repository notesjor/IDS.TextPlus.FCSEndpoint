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
    public abstract void ProcessRequest(HttpContext ctx);
  }
}
