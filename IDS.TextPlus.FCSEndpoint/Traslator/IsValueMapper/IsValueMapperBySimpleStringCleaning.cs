using IDS.TextPlus.FCSEndpoint.Traslator.IsValueMapper.Abstract;

namespace IDS.TextPlus.FCSEndpoint.Traslator.IsValueMapper
{
  public class IsValueMapperBySimpleStringCleaning : AbstractIsValueMapper
  {
    public IsValueMapperBySimpleStringCleaning()
    {
      IsActive = true;
    }

    public override string MapValue(string field, string value)
    {
      var split = value.Split("#");
      if (split.Length > 1)
        return split.Last();
      split = value.Split("/");
      return split.Length > 1 ? split.Last().Replace(".html", "") : value;
    }
  }
}
