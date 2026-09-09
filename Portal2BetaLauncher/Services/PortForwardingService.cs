using System.Diagnostics;

namespace Portal2BetaLauncher.Services;

public sealed record ForwardRule(string Name, string Protocol, int ListenPort, string TargetHost, int TargetPort);

public sealed class PortForwardingService
{
    public async Task<string> AddWindowsTcpPortProxyAsync(ForwardRule rule)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("The port-forwarding helper requires Windows.");
        if (!string.Equals(rule.Protocol, "TCP", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Windows netsh portproxy supports TCP. UDP relay support is intentionally kept separate from router port forwarding.");
        return await RunNetshAsync($"interface portproxy add v4tov4 listenport={rule.ListenPort} listenaddress=0.0.0.0 connectport={rule.TargetPort} connectaddress={rule.TargetHost}");
    }

    public async Task<string> RemoveWindowsTcpPortProxyAsync(ForwardRule rule) =>
        await RunNetshAsync($"interface portproxy delete v4tov4 listenport={rule.ListenPort} listenaddress=0.0.0.0");

    public async Task<string> ListWindowsTcpPortProxiesAsync() => await RunNetshAsync("interface portproxy show all");

    private static async Task<string> RunNetshAsync(string args)
    {
        using var p = Process.Start(new ProcessStartInfo("netsh.exe", args) { UseShellExecute=false, RedirectStandardOutput=true, RedirectStandardError=true, CreateNoWindow=true })
            ?? throw new InvalidOperationException("Could not start netsh.exe.");
        var output = await p.StandardOutput.ReadToEndAsync();
        var error = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? output : error.Trim());
        return output.Trim();
    }
}
