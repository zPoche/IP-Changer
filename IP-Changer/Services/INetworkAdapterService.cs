using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public interface INetworkAdapterService
{
    IReadOnlyList<NetworkAdapterInfo> RefreshAdapters();

    NetworkAdapterInfo? FindByInterfaceId(string interfaceId);

    string? GetNetshInterfaceName(string interfaceId);
}
