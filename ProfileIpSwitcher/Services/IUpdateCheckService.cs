namespace ProfileIpSwitcher.Services;

public interface IUpdateCheckService
{
    /// <summary>
    /// Prüft auf neuere Version. TODO: echten Endpunkt anbinden (z. B. GitHub API oder version.json).
    /// </summary>
    Task<UpdateCheckResult> CheckAsync(string configuredUrl, CancellationToken cancellationToken = default);
}

public sealed class UpdateCheckResult
{
    public bool Success { get; init; }

    public string CurrentVersion { get; init; } = string.Empty;

    public string? LatestVersion { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? ReleasesPageUrl { get; init; }
}
