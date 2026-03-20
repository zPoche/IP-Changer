namespace ProfileIpSwitcher.Services;

public interface ILoggingService
{
    void Info(string message);

    void Warn(string message);

    void Error(string message, Exception? exception = null);
}
