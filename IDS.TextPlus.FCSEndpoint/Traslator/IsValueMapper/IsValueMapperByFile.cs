using IDS.TextPlus.FCSEndpoint.Traslator.IsValueMapper.Abstract;
using Newtonsoft.Json;
using System.Text;

namespace IDS.TextPlus.FCSEndpoint.Traslator.IsValueMapper
{
  public class IsValueMapperByFile : AbstractIsValueMapper
  {
    private Dictionary<string, Dictionary<string, string>> _mapping = new();

    public IsValueMapperByFile()
    {
      try
      {
        _mapping = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText("Snippets/mapping.json", Encoding.UTF8));
        IsActive = _mapping is { Count: > 0 };
      }
      catch
      {
        IsActive = false;
      }
    }

    public override string MapValue(string field, string value) 
      => !_mapping.ContainsKey(field) ? value : _mapping[field].GetValueOrDefault(value, value);
  }
}
