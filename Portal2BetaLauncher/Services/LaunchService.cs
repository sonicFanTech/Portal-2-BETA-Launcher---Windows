using Portal2BetaLauncher.Models;
using System.Diagnostics;
using System.Text;

namespace Portal2BetaLauncher.Services;

public sealed class LaunchOptions
{
    public string? Map { get; init; }
    public int? Height { get; init; }
    public int? Width { get; init; }
    public bool StartSteam { get; init; }
    public bool Debug { get; init; }
}

public sealed class LaunchService
{
    public Process Start(DetectedBuild build, LaunchOptions options)
    {
        if (!File.Exists(build.ExecutablePath)) throw new FileNotFoundException("The beta executable no longer exists.", build.ExecutablePath);
        if (options.StartSteam) StartSteam();
        var args = build.Build.DefaultArguments(options.Map, options.Height, options.Width);
        var psi = new ProcessStartInfo
        {
            FileName = build.ExecutablePath,
            Arguments = args,
            WorkingDirectory = build.StartDirectory,
            UseShellExecute = false
        };
        return Process.Start(psi) ?? throw new InvalidOperationException("Windows did not return a process handle.");
    }

    public static bool TryFindSteam(out string? path)
    {
        path = null;
        foreach (var key in new[] { Microsoft.Win32.Registry.CurrentUser, Microsoft.Win32.Registry.LocalMachine })
        {
            foreach (var subKeyName in new[] { "Software\\Valve\\Steam", "Software\\WOW6432Node\\Valve\\Steam" })
            {
                try
                {
                    using var keyHandle = key.OpenSubKey(subKeyName);
                    var value = keyHandle?.GetValue("SteamPath") as string ?? keyHandle?.GetValue("InstallPath") as string;
                    if (!string.IsNullOrWhiteSpace(value) && File.Exists(Path.Combine(value, "steam.exe"))) { path = Path.Combine(value, "steam.exe"); return true; }
                }
                catch { }
            }
        }
        return false;
    }

    public static Process? StartSteam()
    {
        if (!TryFindSteam(out var steamExe) || steamExe is null) return null;
        return Process.Start(new ProcessStartInfo(steamExe) { UseShellExecute = true });
    }

    public static string BuildCommandLine(DetectedBuild build, LaunchOptions options) =>
        $"{Path.GetFileName(build.ExecutablePath)} {build.Build.DefaultArguments(options.Map, options.Height, options.Width)}";
}
