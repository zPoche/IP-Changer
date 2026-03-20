namespace ProfileIpSwitcher.Services;

public interface IUpdateCheckService
{
    /// <summary>
    /// Prüft auf neuere Version (GitHub Releases API oder JSON mit latestVersion).
    /// </summary>
    Task<UpdateCheckResult> CheckAsync(string configuredUrl, CancellationToken cancellationToken = default);
}

public sealed class UpdateCheckResult
{
    public bool Success { get; init; }

    public string CurrentVersion { get; init; } = string.Empty;

    public string? LatestVersion { get; init; }

    /// <summary>True, wenn die Remote-Version neuer ist als die installierte.</summary>
    public bool UpdateAvailable { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? ReleasesPageUrl { get; init; }
}
