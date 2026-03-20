using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public interface IProfilePersistenceService
{
    ProfilesDocument Load();

    void Save(ProfilesDocument document);
}
