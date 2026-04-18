using System.Text;

namespace AgentBrowser.Diagnostics;

public static class TimestampedFileLog
{
    public static void Write(string logPath, string message)
    {
        try
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(logPath, line, new UTF8Encoding(false));
        }
        catch
        {
            // Ignore logging failures.
        }
    }

    public static void WriteException(string logPath, string context, Exception exception)
    {
        Write(logPath, $"{context}: {exception}");
    }
}
