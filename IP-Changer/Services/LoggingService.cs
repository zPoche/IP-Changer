using System.IO;

namespace ProfileIpSwitcher.Services;

public sealed class LoggingService : ILoggingService
{
    public static LoggingService Shared { get; } = new();

    private const long MaxBytesBeforeRotate = 5 * 1024 * 1024;
    private static readonly object Gate = new();

    private readonly string _logFilePath;

    public LoggingService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProfileIpSwitcher",
            "logs");
        Directory.CreateDirectory(dir);
        _logFilePath = Path.Combine(dir, "app.log");
    }

    public void Info(string message) => Write("INFO", message, null);

    public void Warn(string message) => Write("WARN", message, null);

    public void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private void Write(string level, string message, Exception? ex)
    {
        try
        {
            lock (Gate)
            {
                RotateIfNeeded();
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level} {message}";
                if (ex != null)
                    line += Environment.NewLine + ex;
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
        }
        catch
        {
            /* Logging darf nicht abstürzen */
        }
    }

    private void RotateIfNeeded()
    {
        try
        {
            var fi = new FileInfo(_logFilePath);
            if (!fi.Exists || fi.Length < MaxBytesBeforeRotate) return;
            var rolled = _logFilePath + "." + DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Move(_logFilePath, rolled);
        }
        catch
        {
            /* ignore */
        }
    }
}
