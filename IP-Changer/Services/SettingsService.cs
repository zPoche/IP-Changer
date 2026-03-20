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
                return new AppSettings();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            _log.Error("Einstellungen laden fehlgeschlagen.", ex);
            return new AppSettings();
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
}
