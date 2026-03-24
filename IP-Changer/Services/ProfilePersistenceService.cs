using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public sealed class ProfilePersistenceService : IProfilePersistenceService
{
    private readonly ILoggingService _log;

    public static JsonSerializerOptions CreateJsonOptions() => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public ProfilePersistenceService(ILoggingService log)
    {
        _log = log;
    }

    public ProfilesDocument Load()
    {
        try
        {
            var path = AppPaths.ProfilesPath;
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(AppPaths.AppDataFolder);
                return new ProfilesDocument();
            }

            var json = File.ReadAllText(path);
            var doc = JsonSerializer.Deserialize<ProfilesDocument>(json, JsonOptions);
            if (doc == null)
            {
                _log.Warn("Profile: Datei konnte nicht deserialisiert werden.");
                return RecoverFromCorruptProfiles(path, "Deserialisierung ergab null.");
            }

            _log.Info($"Profile geladen: {doc.Profiles.Count} Profil(e).");
            return doc;
        }
        catch (Exception ex)
        {
            _log.Error("Profile laden fehlgeschlagen.", ex);
            return RecoverFromCorruptProfiles(AppPaths.ProfilesPath, ex.Message);
        }
    }

    public void Save(ProfilesDocument document)
    {
        Directory.CreateDirectory(AppPaths.AppDataFolder);
        var path = AppPaths.ProfilesPath;
        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(path, json);
        _log.Info("Profile gespeichert.");
    }

    private ProfilesDocument RecoverFromCorruptProfiles(string sourcePath, string reason)
    {
        try
        {
            if (File.Exists(sourcePath))
            {
                Directory.CreateDirectory(AppPaths.AppDataFolder);
                var backup = Path.Combine(
                    AppPaths.AppDataFolder,
                    $"profiles.corrupt.{DateTime.Now:yyyyMMddHHmmss}.json");
                File.Copy(sourcePath, backup, overwrite: false);
                _log.Warn($"Profile: korrupt/ungültig ({reason}). Backup erstellt: {backup}");
            }
        }
        catch (Exception backupEx)
        {
            _log.Warn("Profile: Backup der korrupten Datei fehlgeschlagen: " + backupEx.Message);
        }

        return new ProfilesDocument();
    }
}
