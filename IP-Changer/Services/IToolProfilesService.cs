using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public interface IToolProfilesService
{
    ToolProfilesDocument Load();

    void Save(ToolProfilesDocument document);
}
