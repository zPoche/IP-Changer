using System.Reflection;

namespace ProfileIpSwitcher.Services;

public sealed class UpdateCheckService : IUpdateCheckService
{
    private readonly ILoggingService _log;
    private readonly ISettingsService _settings;

    public UpdateCheckService(ILoggingService log, ISettingsService settings)
    {
        _log = log;
        _settings = settings;
    }

    public Task<UpdateCheckResult> CheckAsync(string configuredUrl, CancellationToken cancellationToken = default)
    {
        // TODO: HTTP GET auf version.json oder GitHub Releases API; hier nur Platzhalter.
        var current = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        _log.Info($"Update-Check (Stub) URL={configuredUrl}");

        var settings = _settings.Load();
        var result = new UpdateCheckResult
        {
            Success = true,
            CurrentVersion = current,
            LatestVersion = null,
            Message =
                "Update-Prüfung ist noch nicht angebunden. Konfigurieren Sie eine URL in den Einstellungen und implementieren Sie den Abruf in UpdateCheckService.",
            ReleasesPageUrl = settings.GitHubReleasesUrl
        };

        return Task.FromResult(result);
    }
}
