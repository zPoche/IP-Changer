using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public sealed class ToolProfilesService : IToolProfilesService
{
    private readonly ILoggingService _log;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ToolProfilesService(ILoggingService log)
    {
        _log = log;
    }

    public ToolProfilesDocument Load()
    {
        try
        {
            var path = AppPaths.ToolProfilesPath;
            if (!File.Exists(path))
                return new ToolProfilesDocument();

            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<ToolProfilesDocument>(json, JsonOptions);
            return loaded ?? new ToolProfilesDocument();
        }
        catch (Exception ex)
        {
            _log.Error("Tool-Profile laden fehlgeschlagen.", ex);
            return new ToolProfilesDocument();
        }
    }

    public void Save(ToolProfilesDocument document)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.AppDataFolder);
            var json = JsonSerializer.Serialize(document, JsonOptions);
            File.WriteAllText(AppPaths.ToolProfilesPath, json);
        }
        catch (Exception ex)
        {
            _log.Error("Tool-Profile speichern fehlgeschlagen.", ex);
        }
    }
}
