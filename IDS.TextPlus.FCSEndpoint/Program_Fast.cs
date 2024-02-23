using IDS.TextPlus.FCSEndpoint.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tfres;

namespace IDS.TextPlus.FCSEndpoint
{
  /// <summary>
  /// This is only for IDS interal use only - this API is fast, simple and cames without the FCS bounderies  
  /// </summary>
  internal partial class Program
  {
    /// <summary>
    /// MIME-Type for response
    /// </summary>
    protected string _mime = "application/xml;charset=utf-8";

    private static void FastInfo(HttpContext context)
    {
      throw new NotImplementedException();
    }

    private static void FastInfoScan(HttpContext context)
    {
      throw new NotImplementedException();
    }

    private static void FastScan(HttpContext context)
    {
      throw new NotImplementedException();
    }

    private static void FastSearch(HttpContext context)
    {
      
    }
  }
}
