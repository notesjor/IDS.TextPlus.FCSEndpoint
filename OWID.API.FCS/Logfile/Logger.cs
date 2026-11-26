using System.Text;

namespace OWID.API.FCS.Logfile
{
  public static class Logger
  {
    private static string _logfilePath = Path.Combine(AppContext.BaseDirectory, "log.txt");

    private static FileStream _fileStream = new FileStream(_logfilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
    private static StreamWriter _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8) { AutoFlush = true };
    private static object _lock = new object();

    public static LogfileEntry Create(string query, int from, int max, bool provideDataView)
    {
      return new LogfileEntry
      {
        Timestamp = DateTime.Now,
        Query = query,
        From = from,
        Max = max,
        ProvideDataView = provideDataView
      };
    }

    public static void Success(LogfileEntry entry, int hits)
    {
      entry.ErrorNo = 0;
      entry.Hits = hits;
      WriteLog(entry);
    }

    public static void Error(LogfileEntry entry, int errorNo)
    {
      entry.ErrorNo = errorNo;
      entry.Hits = 0;
      WriteLog(entry);
    }

    private static void WriteLog(LogfileEntry entry)
    {
      try
      {
        lock (_lock)
        {
          _streamWriter.WriteLine(entry.ToString());
        }
      }
      catch
      {
        // ignore
      }
    }
  }
}
