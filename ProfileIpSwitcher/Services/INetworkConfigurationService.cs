using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public interface INetworkConfigurationService
{
    Task<NetConfigurationResult> ApplyProfileAsync(NetworkProfile profile, string netshInterfaceName,
        CancellationToken cancellationToken = default);
}
