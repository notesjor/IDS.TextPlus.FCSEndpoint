namespace IDS.TextPlus.FCSEndpoint.Traslator.IsValueMapper.Abstract
{
  public abstract class AbstractIsValueMapper
  {
    public abstract string MapValue(string field, string value);

    public bool IsActive { get; set; } = false;
  }
}
