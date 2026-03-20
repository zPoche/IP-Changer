using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using ManagementObjectSearcher = System.Management.ManagementObjectSearcher;
using ManagementObject = System.Management.ManagementObject;
using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public sealed class NetworkAdapterService : INetworkAdapterService
{
    private readonly ILoggingService _log;
    private List<NetworkAdapterInfo> _cache = new();

    public NetworkAdapterService(ILoggingService log)
    {
        _log = log;
    }

    public IReadOnlyList<NetworkAdapterInfo> RefreshAdapters()
    {
        var list = new List<NetworkAdapterInfo>();
        var enabledByConnectionId = LoadWmiAdapterEnabledMap();
        var categoryByAlias = LoadNetworkCategories();
        var wifiSsids = LoadWifiSsidsByInterfaceName();
        var dhcpByInterfaceId = LoadDhcpEnabledByInterfaceId();

        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                if (!enabledByConnectionId.GetValueOrDefault(ni.Name, true))
                    continue;

                var props = ni.GetIPProperties();
                var ipv4Unicast = props.UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                var ipv4 = ipv4Unicast?.Address.ToString() ?? "—";
                var mask = GetIpv4Mask(ipv4Unicast);

                var gw = props.GatewayAddresses
                    .FirstOrDefault(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                var gateway = gw?.Address.ToString() ?? "—";

                var dns = string.Join(", ",
                    props.DnsAddresses
                        .Where(d => d.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .Select(d => d.ToString()));

                if (string.IsNullOrEmpty(dns)) dns = "—";

                var dhcp = ResolveDhcpEnabled(ni, props, ipv4Unicast, dhcpByInterfaceId);

                var desc = ni.Description;
                var wireless = ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                               || desc.Contains("Wireless", StringComparison.OrdinalIgnoreCase)
                               || desc.Contains("Wi-Fi", StringComparison.OrdinalIgnoreCase)
                               || desc.Contains("WLAN", StringComparison.OrdinalIgnoreCase)
                               || desc.Contains("802.11", StringComparison.OrdinalIgnoreCase);

                var category = categoryByAlias.GetValueOrDefault(ni.Name)
                               ?? categoryByAlias.GetValueOrDefault(desc)
                               ?? "Unbekannt";

                string? ssid = null;
                if (wireless && wifiSsids.TryGetValue(ni.Name, out var s))
                    ssid = s;
                else if (wireless && wifiSsids.Count == 1)
                    ssid = wifiSsids.Values.First();

                list.Add(new NetworkAdapterInfo
                {
                    InterfaceId = ni.Id,
                    Name = ni.Name,
                    Description = desc,
                    OperationalStatus = ni.OperationalStatus.ToString(),
                    IsWireless = wireless,
                    Ipv4 = ipv4,
                    SubnetMask = mask,
                    Gateway = gateway,
                    DnsServers = dns,
                    DhcpEnabled = dhcp,
                    NetworkCategory = category,
                    WifiSsid = ssid
                });
            }
        }
        catch (Exception ex)
        {
            _log.Error("Adapter-Enumeration fehlgeschlagen.", ex);
        }

        _cache = list;
        return list;
    }

    public NetworkAdapterInfo? FindByInterfaceId(string interfaceId) =>
        _cache.FirstOrDefault(a => string.Equals(a.InterfaceId, interfaceId, StringComparison.OrdinalIgnoreCase));

    public string? GetNetshInterfaceName(string interfaceId) => FindByInterfaceId(interfaceId)?.Name;

    /// <summary>
    /// DHCP-Status: zuerst WMI Win32_NetworkAdapterConfiguration.DHCPEnabled (SettingID ↔ NetworkInterface.Id),
    /// dann DHCP-Server-Adressen aus IP-Properties, sonst Fallback-Heuristik.
    /// </summary>
    private static bool ResolveDhcpEnabled(
        NetworkInterface ni,
        IPInterfaceProperties props,
        UnicastIPAddressInformation? ipv4Unicast,
        IReadOnlyDictionary<string, bool> wmiDhcpByNormalizedId)
    {
        var key = NormalizeNetworkInterfaceId(ni.Id);
        if (!string.IsNullOrEmpty(key) && wmiDhcpByNormalizedId.TryGetValue(key, out var wmi))
            return wmi;

        if (props.DhcpServerAddresses.Count > 0)
            return true;

        return FallbackDhcpHeuristic(ni, ipv4Unicast);
    }

    /// <summary>WMI SettingID auf normalisierte Interface-GUID abbilden (wie <c>NetworkInterface.Id</c>).</summary>
    private Dictionary<string, bool> LoadDhcpEnabledByInterfaceId()
    {
        var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT SettingID, DHCPEnabled FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE");
            foreach (ManagementObject mo in searcher.Get())
            {
                var settingId = mo["SettingID"]?.ToString();
                if (string.IsNullOrWhiteSpace(settingId)) continue;
                var key = NormalizeNetworkInterfaceId(settingId);
                if (string.IsNullOrEmpty(key)) continue;
                map[key] = mo["DHCPEnabled"] is bool b && b;
            }
        }
        catch (Exception ex)
        {
            _log.Warn("WMI Win32_NetworkAdapterConfiguration (DHCPEnabled): " + ex.Message);
        }

        return map;
    }

    private static string NormalizeNetworkInterfaceId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return string.Empty;
        var s = id.Trim();
        if (s.StartsWith('{') && s.EndsWith('}'))
            s = s.TrimStart('{').TrimEnd('}');
        return s;
    }

    private static bool FallbackDhcpHeuristic(NetworkInterface ni, UnicastIPAddressInformation? uni)
    {
        try
        {
            if (uni == null) return true;
            return ni.OperationalStatus == OperationalStatus.Up;
        }
        catch
        {
            return false;
        }
    }

    private Dictionary<string, bool> LoadWmiAdapterEnabledMap()
    {
        var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT NetConnectionID, NetEnabled FROM Win32_NetworkAdapter WHERE NetConnectionID IS NOT NULL");
            foreach (ManagementObject mo in searcher.Get())
            {
                var id = mo["NetConnectionID"]?.ToString();
                if (string.IsNullOrEmpty(id)) continue;
                var enabled = mo["NetEnabled"] is bool b && b;
                map[id] = enabled;
            }
        }
        catch (Exception ex)
        {
            _log.Warn("WMI Win32_NetworkAdapter: " + ex.Message);
        }

        return map;
    }

    private Dictionary<string, string> LoadNetworkCategories()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\StandardCimv2",
                "SELECT InterfaceAlias, NetworkCategory FROM MSFT_NetConnectionProfile");
            foreach (ManagementObject mo in searcher.Get())
            {
                var alias = mo["InterfaceAlias"]?.ToString();
                var cat = mo["NetworkCategory"];
                if (string.IsNullOrEmpty(alias)) continue;
                var label = cat switch
                {
                    0 => "Öffentlich",
                    1 => "Privat",
                    2 => "Domäne",
                    _ => cat?.ToString() ?? "Unbekannt"
                };
                map[alias] = label;
            }
        }
        catch (Exception ex)
        {
            _log.Warn("WMI MSFT_NetConnectionProfile: " + ex.Message);
        }

        return map;
    }

    private static Dictionary<string, string> LoadWifiSsidsByInterfaceName()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "wlan show interfaces",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var p = Process.Start(psi);
            if (p == null) return result;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(15000);

            string? currentName = null;
            foreach (var raw in output.Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                var mName = Regex.Match(line, @"^\s*Name\s*:\s*(.+)$", RegexOptions.IgnoreCase);
                if (mName.Success)
                {
                    currentName = mName.Groups[1].Value.Trim();
                    continue;
                }

                var mSsid = Regex.Match(line, @"^\s*SSID\s*:\s*(.+)$", RegexOptions.IgnoreCase);
                if (mSsid.Success && currentName != null)
                {
                    var ssid = mSsid.Groups[1].Value.Trim();
                    if (!ssid.Equals("none", StringComparison.OrdinalIgnoreCase))
                        result[currentName] = ssid;
                }
            }
        }
        catch
        {
            /* ignore */
        }

        return result;
    }

    private static string GetIpv4Mask(UnicastIPAddressInformation? uni)
    {
        if (uni == null) return "—";
        try
        {
            return Ipv4MaskFromPrefix(uni.PrefixLength);
        }
        catch
        {
            return "—";
        }
    }

    private static string Ipv4MaskFromPrefix(int prefixLength)
    {
        if (prefixLength is < 0 or > 32) return "—";
        if (prefixLength == 0) return "0.0.0.0";
        var shift = 32 - prefixLength;
        var mask = shift == 32 ? 0u : uint.MaxValue << shift;
        var bytes = BitConverter.GetBytes(mask);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return new IPAddress(bytes).ToString();
    }
}
