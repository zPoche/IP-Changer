using System.Diagnostics;
using System.Text;
using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public sealed class NetworkConfigurationService : INetworkConfigurationService
{
    private readonly ILoggingService _log;

    public NetworkConfigurationService(ILoggingService log)
    {
        _log = log;
    }

    public async Task<NetConfigurationResult> ApplyProfileAsync(NetworkProfile profile, string netshInterfaceName,
        CancellationToken cancellationToken = default)
    {
        var sbOut = new StringBuilder();
        var sbErr = new StringBuilder();

        try
        {
            if (string.IsNullOrWhiteSpace(netshInterfaceName))
                return Fail("Kein Adaptername für netsh.", sbOut, sbErr);

            var name = netshInterfaceName.Trim();

            if (profile.Mode == IpAddressMode.Dhcp)
            {
                var r1 = await RunNetshAsync(
                    $"interface ip set address name=\"{Escape(name)}\" source=dhcp", cancellationToken);
                sbOut.AppendLine(r1.stdout);
                sbErr.AppendLine(r1.stderr);
                if (r1.exit != 0)
                    return Fail($"DHCP Adresse fehlgeschlagen (Exit {r1.exit}).", sbOut, sbErr);

                var r2 = await RunNetshAsync(
                    $"interface ip set dns name=\"{Escape(name)}\" source=dhcp", cancellationToken);
                sbOut.AppendLine(r2.stdout);
                sbErr.AppendLine(r2.stderr);
                if (r2.exit != 0)
                    return Fail($"DHCP DNS fehlgeschlagen (Exit {r2.exit}).", sbOut, sbErr);

                return Ok("Profil (DHCP) angewendet.", sbOut, sbErr);
            }

            var ip = profile.Ipv4?.Trim() ?? "";
            var mask = profile.SubnetMask?.Trim() ?? "";
            var gw = string.IsNullOrWhiteSpace(profile.Gateway) ? "none" : profile.Gateway!.Trim();

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(mask))
                return Fail("Statisches Profil: IP und Subnetzmaske sind erforderlich.", sbOut, sbErr);

            var addrCmd =
                $"interface ip set address name=\"{Escape(name)}\" source=static addr={ip} mask={mask} gateway={gw} gwmetric=1";
            var ra = await RunNetshAsync(addrCmd, cancellationToken);
            sbOut.AppendLine(ra.stdout);
            sbErr.AppendLine(ra.stderr);
            if (ra.exit != 0)
                return Fail($"Statische Adresse fehlgeschlagen (Exit {ra.exit}).", sbOut, sbErr);

            var delDns = await RunNetshAsync($"interface ip delete dns name=\"{Escape(name)}\" all", cancellationToken);
            sbOut.AppendLine(delDns.stdout);
            sbErr.AppendLine(delDns.stderr);

            var dnsList = profile.DnsServers
                .Select(d => d.Address?.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToList();

            if (dnsList.Count == 0)
            {
                var rdhcp = await RunNetshAsync($"interface ip set dns name=\"{Escape(name)}\" source=dhcp",
                    cancellationToken);
                sbOut.AppendLine(rdhcp.stdout);
                sbErr.AppendLine(rdhcp.stderr);
            }
            else
            {
                var rDns1 = await RunNetshAsync(
                    $"interface ip set dns name=\"{Escape(name)}\" static {dnsList[0]} primary", cancellationToken);
                sbOut.AppendLine(rDns1.stdout);
                sbErr.AppendLine(rDns1.stderr);
                if (rDns1.exit != 0)
                    return Fail($"Primärer DNS fehlgeschlagen (Exit {rDns1.exit}).", sbOut, sbErr);

                for (var i = 1; i < dnsList.Count; i++)
                {
                    var idx = i + 1;
                    var rAdd = await RunNetshAsync(
                        $"interface ip add dns name=\"{Escape(name)}\" {dnsList[i]} index={idx}", cancellationToken);
                    sbOut.AppendLine(rAdd.stdout);
                    sbErr.AppendLine(rAdd.stderr);
                }
            }

            _log.Info($"Profil angewendet (statisch) auf Adapter '{name}'.");
            return Ok("Profil (statisch) angewendet.", sbOut, sbErr);
        }
        catch (Exception ex)
        {
            _log.Error("ApplyProfile", ex);
            return Fail(ex.Message, sbOut, sbErr);
        }
    }

    private static string Escape(string name) => name.Replace("\"", "\"\"");

    private static NetConfigurationResult Ok(string msg, StringBuilder stdout, StringBuilder stderr) =>
        new()
        {
            Success = true,
            Message = msg,
            StandardOutput = stdout.ToString(),
            StandardError = stderr.ToString()
        };

    private static NetConfigurationResult Fail(string msg, StringBuilder stdout, StringBuilder stderr) =>
        new()
        {
            Success = false,
            Message = msg,
            StandardOutput = stdout.ToString(),
            StandardError = stderr.ToString()
        };

    private async Task<(int exit, string stdout, string stderr)> RunNetshAsync(string args,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var p = new Process { StartInfo = psi };
        p.Start();
        var readOut = p.StandardOutput.ReadToEndAsync(cancellationToken);
        var readErr = p.StandardError.ReadToEndAsync(cancellationToken);
        await Task.WhenAll(p.WaitForExitAsync(cancellationToken), readOut, readErr).ConfigureAwait(false);
        var o = await readOut.ConfigureAwait(false);
        var e = await readErr.ConfigureAwait(false);
        _log.Info($"netsh {args} => Exit {p.ExitCode}");
        return (p.ExitCode, o, e);
    }
}
