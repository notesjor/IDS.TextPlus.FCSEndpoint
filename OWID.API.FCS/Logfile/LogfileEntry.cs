namespace OWID.API.FCS.Logfile
{
  public sealed class LogfileEntry
  {
    public DateTime Timestamp { get; set; }

    public string Query { get; set; }

    public int From { get; set; }

    public int Max { get; set; }

    public int ErrorNo { get; set; } = 0;

    public int Hits { get; set; } = 0;

    public bool ProvideDataView { get; set; }

    public override string ToString()
    {
      return $"{Timestamp:yyyy-MM-dd HH:mm:ss}\t{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{From}\t{Max}\t{ErrorNo}\t{Hits}\t{ProvideDataView}\t{Query}";
    }
  }
}
