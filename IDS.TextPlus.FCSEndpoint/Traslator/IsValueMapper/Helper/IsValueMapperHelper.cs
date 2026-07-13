using IDS.TextPlus.FCSEndpoint.Traslator.IsValueMapper.Abstract;

namespace IDS.TextPlus.FCSEndpoint.Traslator.IsValueMapper.Helper
{
  public static class IsValueMapperHelper
  {
    private static AbstractIsValueMapper[] _mapper =
    [
      new IsValueMapperByFile(),
      new IsValueMapperBySimpleStringCleaning()
    ];

    private static AbstractIsValueMapper? _bestMapper = null;

    private static AbstractIsValueMapper FindBestMapper()
    {
      foreach (var mapper in _mapper)
        if (mapper.IsActive)
          return mapper;
      return _mapper.Last();
    }

    public static AbstractIsValueMapper BestMapper
    {
      get
      {
        if (_bestMapper == null)
          _bestMapper = FindBestMapper();
        return _bestMapper;
      }
    }
  }
}
