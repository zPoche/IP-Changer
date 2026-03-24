namespace ProfileIpSwitcher.Services;

public interface IUpdateCheckService
{
    /// <summary>
    /// Prüft auf neuere Version (GitHub Releases API oder JSON mit latestVersion).
    /// </summary>
    Task<UpdateCheckResult> CheckAsync(string configuredUrl, CancellationToken cancellationToken = default);
}
