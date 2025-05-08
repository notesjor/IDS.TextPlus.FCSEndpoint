using System.Text;
using IDS.TextPlus.FCSEndpoint.Model;
using Newtonsoft.Json;

namespace IDS.TextPlus.FCSEndpoint.Helper
{
  public static class SearchResourceHelper
  {
    public static Dictionary<string, SearchResouce>? _catalog = null;
    public static Dictionary<string, string>? _keyToPid = null;
    public static Dictionary<string, string>? _pidToKey = null;

    private static Dictionary<string, SearchResouce> Load(string path)
      => JsonConvert.DeserializeObject<Dictionary<string, SearchResouce>>(File.ReadAllText(path, Encoding.UTF8)) ?? new Dictionary<string, SearchResouce>();

    public static Dictionary<string, SearchResouce> Catalog 
      => _catalog ??= Load("Snippets/catalog.json");

    public static Dictionary<string, string> KeyToPid
      => _keyToPid ??= Catalog.ToDictionary(x => x.Key, x => x.Value.Pid);

    public static Dictionary<string, string> PidToKey
      => _pidToKey ??= Catalog.ToDictionary(x => x.Value.Pid, x => x.Key);
  }
}
