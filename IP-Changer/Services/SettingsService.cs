using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly ILoggingService _log;
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "ProfileIpSwitcher";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SettingsService(ILoggingService log)
    {
        _log = log;
    }

    public AppSettings Load()
    {
        try
        {
            var path = AppPaths.SettingsPath;
            if (!File.Exists(path))
            {
                _log.Info("Einstellungen: keine Datei vorhanden, Standardwerte werden verwendet.");
                return CreateValidatedDefaults();
            }
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (loaded == null)
            {
                _log.Warn("Einstellungen: Datei konnte nicht deserialisiert werden, Standardwerte werden verwendet.");
                return RecoverFromCorruptSettings(path, "Deserialisierung ergab null.");
            }

            var validated = ValidateAndNormalize(loaded);
            _log.Info("Einstellungen geladen.");
            return validated;
        }
        catch (Exception ex)
        {
            _log.Error("Einstellungen laden fehlgeschlagen.", ex);
            return RecoverFromCorruptSettings(AppPaths.SettingsPath, ex.Message);
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(AppPaths.AppDataFolder);
        File.WriteAllText(AppPaths.SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
        ApplyStartWithWindows(settings.StartWithWindows);
        _log.Info("Einstellungen gespeichert.");
    }

    private static void ApplyStartWithWindows(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            if (key == null) return;
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath)) return;

            if (enable)
                key.SetValue(RunValueName, $"\"{exePath}\"");
            else
                key.DeleteValue(RunValueName, throwOnMissingValue: false);
        }
        catch
        {
            /* ignore */
        }
    }

    private AppSettings RecoverFromCorruptSettings(string sourcePath, string reason)
    {
        try
        {
            if (File.Exists(sourcePath))
            {
                Directory.CreateDirectory(AppPaths.AppDataFolder);
                var backup = Path.Combine(
                    AppPaths.AppDataFolder,
                    $"settings.corrupt.{DateTime.Now:yyyyMMddHHmmss}.json");
                File.Copy(sourcePath, backup, overwrite: false);
                _log.Warn($"Einstellungen: korrupt/ungültig erkannt ({reason}). Backup erstellt: {backup}");
            }
        }
        catch (Exception backupEx)
        {
            _log.Warn("Einstellungen: Backup der korrupten Datei fehlgeschlagen: " + backupEx.Message);
        }

        var defaults = CreateValidatedDefaults();
        try
        {
            Save(defaults);
            _log.Info("Einstellungen: Standarddatei wurde neu erstellt.");
        }
        catch (Exception saveEx)
        {
            _log.Warn("Einstellungen: Standarddatei konnte nicht gespeichert werden: " + saveEx.Message);
        }

        return defaults;
    }

    private static AppSettings CreateValidatedDefaults() =>
        ValidateAndNormalize(new AppSettings
        {
            // Bei Startup weiterhin defensiv: kein stilles Wegminimieren.
            MinimizeToTrayInsteadOfClose = false
        });

    private static AppSettings ValidateAndNormalize(AppSettings? settings)
    {
        if (settings == null) return new AppSettings();

        settings.UpdateCheckUrl = NormalizeOrDefault(
            settings.UpdateCheckUrl,
            "https://github.com/zPoche/IP-Changer");
        settings.GitHubReleasesUrl = NormalizeOrDefault(
            settings.GitHubReleasesUrl,
            "https://github.com/zPoche/IP-Changer/releases");

        return settings;
    }

    private static string NormalizeOrDefault(string? value, string fallback)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
    }
}
