using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Input;
using ProfileIpSwitcher.Services;

namespace ProfileIpSwitcher.ViewModels;

public sealed class ToolsViewModel : ViewModelBase
{
    private readonly ILoggingService _log;
    private readonly Action<string> _setLastOperation;

    private string _pingTarget = "8.8.8.8";
    private string _pingResult = "—";
    private bool _isPinging;
    private string _portScanTarget = "127.0.0.1";
    private string _portScanPorts = "22,80,443";
    private string _portScanResult = "—";
    private bool _isPortScanning;
    private string _wakeOnLanMac = string.Empty;
    private string _wakeOnLanBroadcast = "255.255.255.255";
    private string _wakeOnLanPort = "9";
    private string _wakeOnLanResult = "—";
    private bool _isSendingWakeOnLan;

    public ToolsViewModel(ILoggingService log, Action<string> setLastOperation)
    {
        _log = log;
        _setLastOperation = setLastOperation;

        PingCommand = new AsyncRelayCommand(_ => PingAsync(), _ => CanPing);
        PortScanCommand = new AsyncRelayCommand(_ => PortScanAsync(), _ => CanPortScan);
        WakeOnLanCommand = new AsyncRelayCommand(_ => SendWakeOnLanAsync(), _ => CanWakeOnLan);
    }

    public bool CanPing => !IsPinging && !string.IsNullOrWhiteSpace(PingTarget);

    public bool CanPortScan =>
        !IsPortScanning &&
        !string.IsNullOrWhiteSpace(PortScanTarget) &&
        !string.IsNullOrWhiteSpace(PortScanPorts);

    public bool CanWakeOnLan =>
        !IsSendingWakeOnLan &&
        !string.IsNullOrWhiteSpace(WakeOnLanMac) &&
        !string.IsNullOrWhiteSpace(WakeOnLanBroadcast) &&
        !string.IsNullOrWhiteSpace(WakeOnLanPort);

    public string PingTarget
    {
        get => _pingTarget;
        set
        {
            if (SetProperty(ref _pingTarget, value))
            {
                Raise(nameof(CanPing));
                ((AsyncRelayCommand)PingCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string PingResult
    {
        get => _pingResult;
        set => SetProperty(ref _pingResult, value);
    }

    public bool IsPinging
    {
        get => _isPinging;
        set
        {
            if (SetProperty(ref _isPinging, value))
            {
                Raise(nameof(CanPing));
                ((AsyncRelayCommand)PingCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string PortScanTarget
    {
        get => _portScanTarget;
        set
        {
            if (SetProperty(ref _portScanTarget, value))
            {
                Raise(nameof(CanPortScan));
                ((AsyncRelayCommand)PortScanCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string PortScanPorts
    {
        get => _portScanPorts;
        set
        {
            if (SetProperty(ref _portScanPorts, value))
            {
                Raise(nameof(CanPortScan));
                ((AsyncRelayCommand)PortScanCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string PortScanResult
    {
        get => _portScanResult;
        set => SetProperty(ref _portScanResult, value);
    }

    public bool IsPortScanning
    {
        get => _isPortScanning;
        set
        {
            if (SetProperty(ref _isPortScanning, value))
            {
                Raise(nameof(CanPortScan));
                ((AsyncRelayCommand)PortScanCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string WakeOnLanMac
    {
        get => _wakeOnLanMac;
        set
        {
            if (SetProperty(ref _wakeOnLanMac, value))
            {
                Raise(nameof(CanWakeOnLan));
                ((AsyncRelayCommand)WakeOnLanCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string WakeOnLanBroadcast
    {
        get => _wakeOnLanBroadcast;
        set
        {
            if (SetProperty(ref _wakeOnLanBroadcast, value))
            {
                Raise(nameof(CanWakeOnLan));
                ((AsyncRelayCommand)WakeOnLanCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string WakeOnLanPort
    {
        get => _wakeOnLanPort;
        set
        {
            if (SetProperty(ref _wakeOnLanPort, value))
            {
                Raise(nameof(CanWakeOnLan));
                ((AsyncRelayCommand)WakeOnLanCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string WakeOnLanResult
    {
        get => _wakeOnLanResult;
        set => SetProperty(ref _wakeOnLanResult, value);
    }

    public bool IsSendingWakeOnLan
    {
        get => _isSendingWakeOnLan;
        set
        {
            if (SetProperty(ref _isSendingWakeOnLan, value))
            {
                Raise(nameof(CanWakeOnLan));
                ((AsyncRelayCommand)WakeOnLanCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public ICommand PingCommand { get; }
    public ICommand PortScanCommand { get; }
    public ICommand WakeOnLanCommand { get; }

    private async Task PingAsync()
    {
        var target = PingTarget.Trim();
        if (!TryResolveTarget(target, out var resolvedIp, out var resolveError))
        {
            PingResult = resolveError;
            _setLastOperation("Ping fehlgeschlagen: " + resolveError);
            return;
        }

        IsPinging = true;
        PingResult = $"Ping läuft zu {target} ({resolvedIp}) …";
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(resolvedIp, 2000);
            if (reply.Status == IPStatus.Success)
            {
                var replyAddress = reply.Address?.ToString() ?? resolvedIp.ToString();
                PingResult = $"Antwort von {replyAddress}: Zeit={reply.RoundtripTime} ms";
                _setLastOperation($"Ping erfolgreich: {replyAddress} ({reply.RoundtripTime} ms).");
                _log.Info($"Ping erfolgreich: {replyAddress} in {reply.RoundtripTime} ms.");
                return;
            }

            PingResult = $"Keine Antwort von {target} ({reply.Status}).";
            _setLastOperation($"Ping fehlgeschlagen: {reply.Status}.");
            _log.Warn($"Ping fehlgeschlagen ({target}): {reply.Status}.");
        }
        catch (PingException ex)
        {
            PingResult = $"Ping-Fehler: {ex.InnerException?.Message ?? ex.Message}";
            _setLastOperation("Ping fehlgeschlagen.");
            _log.Warn("Ping-Fehler: " + ex.Message);
        }
        catch (Exception ex)
        {
            PingResult = $"Unerwarteter Fehler: {ex.Message}";
            _setLastOperation("Ping fehlgeschlagen.");
            _log.Error("Ping", ex);
        }
        finally
        {
            IsPinging = false;
        }
    }

    private async Task PortScanAsync()
    {
        var target = PortScanTarget.Trim();
        if (!TryResolveTarget(target, out var resolvedIp, out var resolveError))
        {
            PortScanResult = resolveError;
            _setLastOperation("Portscan fehlgeschlagen.");
            return;
        }

        if (!TryParsePorts(PortScanPorts, out var ports, out var parseError))
        {
            PortScanResult = parseError;
            _setLastOperation("Portscan fehlgeschlagen.");
            return;
        }

        IsPortScanning = true;
        PortScanResult = $"Scanne {target} ({resolvedIp}) auf {ports.Count} Ports …";
        try
        {
            var openPorts = new List<int>();
            foreach (var port in ports)
            {
                if (await IsTcpPortOpenAsync(resolvedIp, port, timeoutMs: 300))
                    openPorts.Add(port);
            }

            if (openPorts.Count == 0)
            {
                PortScanResult = $"Keine offenen Ports gefunden auf {target} ({resolvedIp}).";
                _setLastOperation("Portscan abgeschlossen: keine offenen Ports.");
                _log.Info($"Portscan abgeschlossen ({target}/{resolvedIp}): keine offenen Ports.");
                return;
            }

            PortScanResult = $"Offene Ports auf {target} ({resolvedIp}): {string.Join(", ", openPorts)}";
            _setLastOperation($"Portscan abgeschlossen: {openPorts.Count} offene Ports.");
            _log.Info($"Portscan abgeschlossen ({target}/{resolvedIp}): {string.Join(", ", openPorts)}");
        }
        catch (Exception ex)
        {
            PortScanResult = $"Portscan-Fehler: {ex.Message}";
            _setLastOperation("Portscan fehlgeschlagen.");
            _log.Error("Portscan", ex);
        }
        finally
        {
            IsPortScanning = false;
        }
    }

    private async Task SendWakeOnLanAsync()
    {
        if (!TryParseMacAddress(WakeOnLanMac, out var macBytes))
        {
            WakeOnLanResult = "Ungültige MAC-Adresse.";
            _setLastOperation("Wake-on-LAN fehlgeschlagen.");
            return;
        }

        if (!IPAddress.TryParse(WakeOnLanBroadcast.Trim(), out var broadcastIp) ||
            broadcastIp.AddressFamily != AddressFamily.InterNetwork)
        {
            WakeOnLanResult = "Ungültige Broadcast-IPv4-Adresse.";
            _setLastOperation("Wake-on-LAN fehlgeschlagen.");
            return;
        }

        if (!int.TryParse(WakeOnLanPort.Trim(), out var port) || port < 1 || port > 65535)
        {
            WakeOnLanResult = "Ungültiger UDP-Port (1-65535).";
            _setLastOperation("Wake-on-LAN fehlgeschlagen.");
            return;
        }

        IsSendingWakeOnLan = true;
        WakeOnLanResult = $"Sende Magic Packet an {WakeOnLanMac.Trim()} …";
        try
        {
            var packet = BuildMagicPacket(macBytes!);
            using var udp = new UdpClient();
            udp.EnableBroadcast = true;
            await udp.SendAsync(packet, packet.Length, new IPEndPoint(broadcastIp, port));

            WakeOnLanResult = $"Magic Packet gesendet an {WakeOnLanMac.Trim()} via {broadcastIp}:{port}.";
            _setLastOperation("Wake-on-LAN gesendet.");
            _log.Info($"Wake-on-LAN gesendet an {WakeOnLanMac.Trim()} via {broadcastIp}:{port}.");
        }
        catch (Exception ex)
        {
            WakeOnLanResult = $"Wake-on-LAN-Fehler: {ex.Message}";
            _setLastOperation("Wake-on-LAN fehlgeschlagen.");
            _log.Error("Wake-on-LAN", ex);
        }
        finally
        {
            IsSendingWakeOnLan = false;
        }
    }

    private static bool TryResolveTarget(string target, out IPAddress resolvedIp, out string error)
    {
        if (IPAddress.TryParse(target, out var ip) && ip.AddressFamily == AddressFamily.InterNetwork)
        {
            resolvedIp = ip;
            error = string.Empty;
            return true;
        }

        try
        {
            var hostEntry = Dns.GetHostEntry(target);
            var ipv4 = hostEntry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            if (ipv4 != null)
            {
                resolvedIp = ipv4;
                error = string.Empty;
                return true;
            }
        }
        catch
        {
            // DNS resolution failed.
        }

        resolvedIp = IPAddress.None;
        error = "Zieladresse konnte nicht aufgelöst werden (IPv4).";
        return false;
    }

    private static bool TryParsePorts(string raw, out List<int> ports, out string error)
    {
        ports = new List<int>();
        var tokens = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            error = "Bitte mindestens einen Port angeben (z. B. 22,80,443 oder 8000-8010).";
            return false;
        }

        var set = new SortedSet<int>();
        foreach (var token in tokens)
        {
            if (token.Contains('-', StringComparison.Ordinal))
            {
                var range = token.Split('-', StringSplitOptions.TrimEntries);
                if (range.Length != 2 ||
                    !int.TryParse(range[0], out var start) ||
                    !int.TryParse(range[1], out var end) ||
                    start < 1 || end > 65535 || start > end)
                {
                    error = $"Ungültiger Portbereich: {token}";
                    return false;
                }

                if (end - start > 2048)
                {
                    error = $"Portbereich zu groß: {token} (max. 2049 Ports pro Bereich).";
                    return false;
                }

                for (var p = start; p <= end; p++)
                    set.Add(p);
            }
            else
            {
                if (!int.TryParse(token, out var singlePort) || singlePort < 1 || singlePort > 65535)
                {
                    error = $"Ungültiger Port: {token}";
                    return false;
                }

                set.Add(singlePort);
            }
        }

        if (set.Count == 0)
        {
            error = "Keine gültigen Ports erkannt.";
            return false;
        }

        if (set.Count > 4096)
        {
            error = "Zu viele Ports ausgewählt (max. 4096 pro Scan).";
            return false;
        }

        ports = set.ToList();
        error = string.Empty;
        return true;
    }

    private static async Task<bool> IsTcpPortOpenAsync(IPAddress target, int port, int timeoutMs)
    {
        using var client = new TcpClient();
        try
        {
            var connectTask = client.ConnectAsync(target, port);
            var timeoutTask = Task.Delay(timeoutMs);
            var completed = await Task.WhenAny(connectTask, timeoutTask);
            if (completed != connectTask) return false;
            await connectTask;
            return client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseMacAddress(string raw, out byte[]? macBytes)
    {
        macBytes = null;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var normalized = new string(raw.Where(Uri.IsHexDigit).ToArray());
        if (normalized.Length != 12) return false;

        var bytes = new byte[6];
        for (var i = 0; i < 6; i++)
        {
            if (!byte.TryParse(normalized.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out bytes[i]))
                return false;
        }

        macBytes = bytes;
        return true;
    }

    private static byte[] BuildMagicPacket(byte[] macBytes)
    {
        var packet = new byte[6 + 16 * macBytes.Length];
        for (var i = 0; i < 6; i++)
            packet[i] = 0xFF;
        for (var i = 6; i < packet.Length; i += macBytes.Length)
            Buffer.BlockCopy(macBytes, 0, packet, i, macBytes.Length);
        return packet;
    }
}
