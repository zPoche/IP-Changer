using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProfileIpSwitcher.Services;

/// <summary>
/// Update-Prüfung über GitHub Releases (<c>/repos/{owner}/{repo}/releases/latest</c>)
/// oder eine JSON-Datei mit Feldern <c>latestVersion</c> / <c>version</c> / <c>latest</c>.
/// </summary>
public sealed class UpdateCheckService : IUpdateCheckService
{
    private const string UserAgent = "ProfileIpSwitcher/1.0 (+https://github.com/zPoche/IP-Changer)";

    private readonly ILoggingService _log;
    private readonly ISettingsService _settings;

    public UpdateCheckService(ILoggingService log, ISettingsService settings)
    {
        _log = log;
        _settings = settings;
    }

    public async Task<UpdateCheckResult> CheckAsync(string configuredUrl, CancellationToken cancellationToken = default)
    {
        var current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0);
        var currentDisplay = current.ToString();

        var settings = _settings.Load();
        var url = (configuredUrl ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(url))
            url = settings.GitHubReleasesUrl.Trim();

        var releasesFallback = !string.IsNullOrWhiteSpace(settings.GitHubReleasesUrl)
            ? settings.GitHubReleasesUrl.Trim()
            : DeriveReleasesPageFromRepoUrl(url);

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
            http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.github+json");
            http.Timeout = TimeSpan.FromSeconds(20);

            string? latestRaw;
            string? releaseHtmlUrl;

            if (IsJsonEndpoint(url))
            {
                _log.Info($"Update-Check: JSON {url}");
                var json = await http.GetStringAsync(url, cancellationToken);
                latestRaw = ParseVersionFromJson(json);
                releaseHtmlUrl = TryReadJsonString(json, "releaseUrl", "releasesUrl", "downloadUrl");
                if (string.IsNullOrEmpty(releaseHtmlUrl))
                    releaseHtmlUrl = releasesFallback;
            }
            else
            {
                var (owner, repo) = ParseGitHubOwnerRepo(url);
                if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                {
                    return new UpdateCheckResult
                    {
                        Success = false,
                        CurrentVersion = currentDisplay,
                        Message =
                            "Ungültige Update-URL. Tragen Sie eine GitHub-Repo-URL ein (z. B. https://github.com/Benutzer/IP-Changer) " +
                            "oder eine direkte JSON-URL (endend mit .json).",
                        ReleasesPageUrl = releasesFallback
                    };
                }

                var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                _log.Info($"Update-Check: GitHub API {apiUrl}");

                using var response = await http.GetAsync(apiUrl, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _log.Warn($"GitHub API {(int)response.StatusCode}: {body}");
                    return new UpdateCheckResult
                    {
                        Success = false,
                        CurrentVersion = currentDisplay,
                        Message = response.StatusCode == System.Net.HttpStatusCode.NotFound
                            ? "Kein GitHub-Release gefunden (noch kein Release im Repository?)."
                            : $"GitHub antwortete mit {(int)response.StatusCode}. Prüfen Sie Repo-Name und Netzwerk.",
                        ReleasesPageUrl = $"https://github.com/{owner}/{repo}/releases"
                    };
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var tag = root.GetProperty("tag_name").GetString() ?? "";
                latestRaw = NormalizeVersionString(tag);
                releaseHtmlUrl = root.TryGetProperty("html_url", out var h) ? h.GetString() : null;
                releaseHtmlUrl ??= $"https://github.com/{owner}/{repo}/releases/latest";
            }

            if (string.IsNullOrWhiteSpace(latestRaw))
            {
                return new UpdateCheckResult
                {
                    Success = false,
                    CurrentVersion = currentDisplay,
                    Message = "Konnte die neueste Versionsnummer nicht auslesen.",
                    ReleasesPageUrl = releasesFallback
                };
            }

            if (!TryParseLooseVersion(latestRaw, out var latestV))
            {
                return new UpdateCheckResult
                {
                    Success = false,
                    CurrentVersion = currentDisplay,
                    LatestVersion = latestRaw,
                    Message = $"Ungültiges Versionsformat: „{latestRaw}“.",
                    ReleasesPageUrl = releaseHtmlUrl ?? releasesFallback
                };
            }

            var updateAvailable = latestV > current;
            var message = updateAvailable
                ? $"Neue Version verfügbar: {latestRaw} (installiert: {currentDisplay})."
                : $"Sie nutzen die aktuelle Version ({currentDisplay}).";

            _log.Info($"Update-Check: remote={latestRaw}, local={currentDisplay}, update={updateAvailable}");

            return new UpdateCheckResult
            {
                Success = true,
                CurrentVersion = currentDisplay,
                LatestVersion = latestRaw,
                UpdateAvailable = updateAvailable,
                Message = message,
                ReleasesPageUrl = releaseHtmlUrl ?? releasesFallback
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log.Error("Update-Check fehlgeschlagen.", ex);
            return new UpdateCheckResult
            {
                Success = false,
                CurrentVersion = currentDisplay,
                Message = "Update-Prüfung fehlgeschlagen: " + ex.Message,
                ReleasesPageUrl = releasesFallback
            };
        }
    }

    private static bool IsJsonEndpoint(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        var u = url.TrimStart().ToLowerInvariant();
        return u.EndsWith(".json", StringComparison.Ordinal) ||
               u.Contains("raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase) &&
               u.Contains(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ParseVersionFromJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            foreach (var prop in new[] { "latestVersion", "version", "latest", "tag" })
            {
                if (root.TryGetProperty(prop, out var el))
                {
                    var s = el.ValueKind == JsonValueKind.String ? el.GetString() : el.GetRawText().Trim('"');
                    if (!string.IsNullOrWhiteSpace(s))
                        return NormalizeVersionString(s!);
                }
            }
        }
        catch
        {
            /* ignore */
        }

        return null;
    }

    private static string? TryReadJsonString(string json, params string[] propertyNames)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            foreach (var prop in propertyNames)
            {
                if (root.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String)
                    return el.GetString();
            }
        }
        catch
        {
            /* ignore */
        }

        return null;
    }

    /// <summary>GitHub-Repo-URL → owner, repo</summary>
    private static (string? Owner, string? Repo) ParseGitHubOwnerRepo(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return (null, null);

        var m = Regex.Match(url.Trim(),
            @"^https?://github\.com/(?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?(?:/|$)",
            RegexOptions.IgnoreCase);
        if (!m.Success) return (null, null);

        var owner = m.Groups["owner"].Value;
        var repo = m.Groups["repo"].Value;
        if (string.Equals(repo, "releases", StringComparison.OrdinalIgnoreCase)) return (null, null);
        return (owner, repo);
    }

    private static string? DeriveReleasesPageFromRepoUrl(string url)
    {
        var (o, r) = ParseGitHubOwnerRepo(url);
        return o != null && r != null ? $"https://github.com/{o}/{r}/releases" : null;
    }

    private static string NormalizeVersionString(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return string.Empty;
        var s = tag.Trim();
        if (s.StartsWith('v') || s.StartsWith('V'))
            s = s[1..].Trim();
        var dash = s.IndexOfAny(['-', '+']);
        if (dash > 0)
            s = s[..dash].Trim();
        return s;
    }

    private static bool TryParseLooseVersion(string s, out Version version)
    {
        version = new Version(0, 0);
        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split('.');
        if (parts.Length == 0) return false;

        try
        {
            var major = int.Parse(parts[0]);
            var minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            var build = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            var rev = parts.Length > 3 ? int.Parse(parts[3]) : 0;
            version = new Version(major, minor, build, rev);
            return true;
        }
        catch
        {
            if (!Version.TryParse(s, out var v) || v is null)
                return false;
            version = v;
            return true;
        }
    }

    /// <summary>Öffnet die Release-URL im Standardbrowser.</summary>
    public static void OpenReleasesPage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
}
