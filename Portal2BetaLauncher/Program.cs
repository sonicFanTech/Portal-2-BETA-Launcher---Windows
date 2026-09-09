using Portal2BetaLauncher.Models;
using Portal2BetaLauncher.Services;
using Portal2BetaLauncher.UI;

namespace Portal2BetaLauncher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length >= 2 && args[0].Equals("--debug-console-pid", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[1], out var pid))
        {
            using var dbg = new WindowsDebugger(); dbg.Output += (_, e) => Console.WriteLine((e.IsError?"[ERROR] ":"") + e.Message);
            dbg.Attach(pid); Console.WriteLine($"Debug console attached to PID {pid}. Press Enter to stop."); Console.ReadLine(); dbg.Stop(); return;
        }
        if (args.Any(a => a.Equals("--gui", StringComparison.OrdinalIgnoreCase)))
        {
            ApplicationConfiguration.Initialize(); Application.Run(new MainForm()); return;
        }
        RunConsole();
    }

    private static void RunConsole()
    {
        var scanner = new BuildScanner(); var launcher = new LaunchService(); var fixes = new FixService(); var steam = new SteamShortcutService(); var net = new PortForwardingService(); var scanRoots = new ScanRootsStore();
        scanner.Status += s => Console.WriteLine($"[scan] {s}");
        IReadOnlyList<DetectedBuild> builds = Array.Empty<DetectedBuild>();
        Console.Title = "Portal 2 Beta Launcher";
        while (true)
        {
            Console.WriteLine(); Console.WriteLine("=== Portal 2 Beta Launcher ===");
            Console.WriteLine("1. List known beta builds"); Console.WriteLine("2. Scan all connected drives + added folders"); Console.WriteLine("3. Scan added folders only"); Console.WriteLine("4. Launch a detected build"); Console.WriteLine("5. Add detected build to Steam"); Console.WriteLine("6. Install a bundled No-Steam fix"); Console.WriteLine("7. Start Steam"); Console.WriteLine("8. Manage scan folders"); Console.WriteLine("9. Port-forwarding helper"); Console.WriteLine("10. GUI mode"); Console.WriteLine("0. Exit"); Console.Write("Select: ");
            switch(Console.ReadLine())
            {
                case "1": foreach(var b in BuildCatalog.All) Console.WriteLine($"{b.BuildNumber,-7} {b.Date,-20} {string.Join(" ",b.StartArguments)}"); break;
                case "2": builds=scanner.ScanAsync(scanRoots.Load(), true).GetAwaiter().GetResult(); PrintBuilds(builds); break;
                case "3": builds=scanner.ScanAsync(scanRoots.Load(), false).GetAwaiter().GetResult(); PrintBuilds(builds); break;
                case "4": LaunchConsole(launcher,builds); break;
                case "5": SteamConsole(steam,builds); break;
                case "6": FixConsole(fixes); break;
                case "7": Console.WriteLine(LaunchService.StartSteam() is null ? "Steam was not found." : "Steam start requested."); break;
                case "8": ManageScanFolders(scanRoots); break;
                case "9": NetConsole(net); break;
                case "10": ApplicationConfiguration.Initialize(); Application.Run(new MainForm()); break;
                case "0": return;
            }
        }
    }

    private static void PrintBuilds(IReadOnlyList<DetectedBuild> builds)
    {
        for (int i = 0; i < builds.Count; i++)
            Console.WriteLine($"[{i}] {builds[i].Build.BuildNumber} {builds[i].ExecutablePath}");
        Console.WriteLine($"Found {builds.Count} match(es).");
    }

    private static void ManageScanFolders(ScanRootsStore store)
    {
        while (true)
        {
            var roots = store.Load();
            Console.WriteLine();
            Console.WriteLine("=== Scan Folders ===");
            if (roots.Count == 0) Console.WriteLine("No saved folders.");
            else for (var i = 0; i < roots.Count; i++) Console.WriteLine($"[{i}] {roots[i]}");
            Console.WriteLine("A. Add folder");
            Console.WriteLine("R. Remove folder");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            var choice = Console.ReadLine();
            if (choice?.Equals("0", StringComparison.OrdinalIgnoreCase) == true) return;
            if (choice?.Equals("a", StringComparison.OrdinalIgnoreCase) == true)
            {
                Console.Write("Folder path: ");
                var path = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    store.Save(roots.Append(path));
                    Console.WriteLine("Folder saved.");
                }
                else Console.WriteLine("Folder does not exist.");
            }
            else if (choice?.Equals("r", StringComparison.OrdinalIgnoreCase) == true)
            {
                Console.Write("Folder index to remove: ");
                if (int.TryParse(Console.ReadLine(), out var index) && index >= 0 && index < roots.Count)
                {
                    store.Save(roots.Where((_, i) => i != index));
                    Console.WriteLine("Folder removed.");
                }
                else Console.WriteLine("Invalid folder index.");
            }
        }
    }

    private static void LaunchConsole(LaunchService launcher,IReadOnlyList<DetectedBuild> builds)
    {
        if(!Pick(builds,out var d)) return; Console.Write("Map (blank for none): "); var map=Console.ReadLine(); Console.Write("Width [1280]: "); int.TryParse(Console.ReadLine(),out var w); if(w<=0)w=1280; Console.Write("Height [720]: "); int.TryParse(Console.ReadLine(),out var h); if(h<=0)h=720; Console.Write("Start Steam first? [y/N]: "); var ss=Console.ReadLine()?.Equals("y",StringComparison.OrdinalIgnoreCase)==true; try { var p=launcher.Start(d,new LaunchOptions{Map=map,Width=w,Height=h,StartSteam=ss}); Console.WriteLine($"Started PID {p.Id}: {LaunchService.BuildCommandLine(d,new LaunchOptions{Map=map,Width=w,Height=h})}"); Console.Write("Open second debugger console? [y/N]: "); if(Console.ReadLine()?.Equals("y",StringComparison.OrdinalIgnoreCase)==true) System.Diagnostics.Process.Start(Environment.ProcessPath!, $"--debug-console-pid {p.Id}"); } catch(Exception ex){Console.WriteLine("ERROR: "+ex.Message);}
    }
    private static void SteamConsole(SteamShortcutService steam,IReadOnlyList<DetectedBuild> builds)
    { if(!Pick(builds,out var d))return; try{steam.AddShortcut(d,new LaunchOptions{Width=1280,Height=720});Console.WriteLine("Shortcut added (shortcuts.vdf was backed up first).");}catch(Exception ex){Console.WriteLine("ERROR: "+ex.Message);} }
    private static void FixConsole(FixService fixes)
    { var names=fixes.GetFixNames(); for(int i=0;i<names.Count;i++)Console.WriteLine($"[{i}] {names[i]}"); Console.Write("Fix: "); if(!int.TryParse(Console.ReadLine(),out var n)||n<0||n>=names.Count)return; Console.Write("Beta root: "); var root=Console.ReadLine(); if(string.IsNullOrWhiteSpace(root))return; try{fixes.Install(names[n],root);Console.WriteLine("Installed. Existing files were backed up in .p2beta-backups.");}catch(Exception ex){Console.WriteLine("ERROR: "+ex.Message);} }
    private static void NetConsole(PortForwardingService net)
    { Console.WriteLine("1. Add TCP proxy  2. Remove TCP proxy  3. List"); switch(Console.ReadLine()){case "1": try{Console.Write("Listen port: ");var l=int.Parse(Console.ReadLine()!);Console.Write("Target host: ");var host=Console.ReadLine()!;Console.Write("Target port: ");var t=int.Parse(Console.ReadLine()!);Console.WriteLine(net.AddWindowsTcpPortProxyAsync(new ForwardRule("Portal2Beta","TCP",l,host,t)).GetAwaiter().GetResult());}catch(Exception ex){Console.WriteLine("ERROR: "+ex.Message);}break;case "2":try{Console.Write("Listen port: ");var l=int.Parse(Console.ReadLine()!);Console.WriteLine(net.RemoveWindowsTcpPortProxyAsync(new ForwardRule("Portal2Beta","TCP",l,"",0)).GetAwaiter().GetResult());}catch(Exception ex){Console.WriteLine("ERROR: "+ex.Message);}break;case "3":try{Console.WriteLine(net.ListWindowsTcpPortProxiesAsync().GetAwaiter().GetResult());}catch(Exception ex){Console.WriteLine("ERROR: "+ex.Message);}break;} }
    private static bool Pick(IReadOnlyList<DetectedBuild> builds,out DetectedBuild d){d=null!;if(builds.Count==0){Console.WriteLine("Scan first.");return false;}Console.Write("Build index: ");if(!int.TryParse(Console.ReadLine(),out var i)||i<0||i>=builds.Count)return false;d=builds[i];return true;}
}
