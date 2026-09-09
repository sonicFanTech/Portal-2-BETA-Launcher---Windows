using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Portal2BetaLauncher.Services;

public sealed record ProcessSnapshot(
    int Pid,
    string Name,
    string Status,
    string Memory,
    string Cpu,
    string WorkingSet,
    string Threads,
    string Handles,
    string Priority,
    string StartTime,
    string WindowTitle,
    string Path,
    string Architecture,
    string Session);

public sealed class ProcessInspector
{
    private readonly Dictionary<int, TimeSpan> previousCpu = new();
    private readonly Dictionary<int, DateTime> previousSample = new();

    public IReadOnlyList<ProcessSnapshot> GetSnapshots()
    {
        var list = new List<ProcessSnapshot>();
        var now = DateTime.UtcNow;
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var cpu = process.TotalProcessorTime;
                var cpuText = "—";
                if (previousCpu.TryGetValue(process.Id, out var oldCpu) && previousSample.TryGetValue(process.Id, out var oldTime))
                {
                    var elapsed = (now - oldTime).TotalMilliseconds;
                    if (elapsed > 0)
                    {
                        var pct = (cpu - oldCpu).TotalMilliseconds / elapsed / Environment.ProcessorCount * 100.0;
                        cpuText = $"{Math.Max(0, pct):0.0}%";
                    }
                }
                previousCpu[process.Id] = cpu;
                previousSample[process.Id] = now;

                string path = "Access denied";
                string arch = "Unknown";
                try
                {
                    path = process.MainModule?.FileName ?? "Unknown";
                    arch = Is64Bit(process) ? "64-bit" : "32-bit";
                }
                catch { }

                var start = "Unknown";
                try { start = process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"); } catch { }
                var priority = "Unknown";
                try { priority = process.PriorityClass.ToString(); } catch { }
                var window = string.IsNullOrWhiteSpace(process.MainWindowTitle) ? "" : process.MainWindowTitle;

                list.Add(new ProcessSnapshot(
                    process.Id,
                    process.ProcessName,
                    process.Responding ? "Running" : "Not responding",
                    FormatBytes(process.PrivateMemorySize64),
                    cpuText,
                    FormatBytes(process.WorkingSet64),
                    process.Threads.Count.ToString(),
                    TryGetHandles(process),
                    priority,
                    start,
                    window,
                    path,
                    arch,
                    TryGetSession(process)));
            }
            catch
            {
            }
            finally
            {
                process.Dispose();
            }
        }

        return list.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ThenBy(p => p.Pid).ToArray();
    }

    public static bool IsLikelyPortal2(ProcessSnapshot snapshot)
    {
        if (!snapshot.Name.Equals("hl2", StringComparison.OrdinalIgnoreCase) &&
            !snapshot.Name.Equals("portal2", StringComparison.OrdinalIgnoreCase))
            return false;

        return snapshot.Path.Contains("portal", StringComparison.OrdinalIgnoreCase) ||
               snapshot.Path.Contains("852_", StringComparison.OrdinalIgnoreCase) ||
               snapshot.Path.Contains("841_", StringComparison.OrdinalIgnoreCase);
    }

    public static string FormatBytes(long value)
    {
        if (value < 1024) return $"{value} B";
        if (value < 1024 * 1024) return $"{value / 1024d:0.0} KB";
        if (value < 1024L * 1024 * 1024) return $"{value / 1024d / 1024d:0.0} MB";
        return $"{value / 1024d / 1024d / 1024d:0.00} GB";
    }

    private static string TryGetHandles(Process p)
    {
        try { return p.HandleCount.ToString(); } catch { return "—"; }
    }

    private static string TryGetSession(Process p)
    {
        try { return p.SessionId.ToString(); } catch { return "—"; }
    }

    private static bool Is64Bit(Process process)
    {
        if (!Environment.Is64BitOperatingSystem) return false;
        if (!IsWow64Process(process.Handle, out var wow64)) return true;
        return !wow64;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);
}
