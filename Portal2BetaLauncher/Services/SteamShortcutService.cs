using System.Text;
using Microsoft.Win32;
using Portal2BetaLauncher.Models;

namespace Portal2BetaLauncher.Services;

public sealed class SteamShortcutService
{
    public string? LocateShortcutsFile()
    {
        var steam = LocateSteamInstall();
        if (steam is null) return null;
        var userdata = Path.Combine(steam, "userdata");
        if (!Directory.Exists(userdata)) return null;
        return Directory.EnumerateDirectories(userdata).Select(d => Path.Combine(d, "config", "shortcuts.vdf")).FirstOrDefault(File.Exists);
    }

    public void AddShortcut(DetectedBuild build, LaunchOptions options)
    {
        var file = LocateShortcutsFile() ?? throw new FileNotFoundException("Could not locate a Steam shortcuts.vdf. Start Steam and sign in first.");
        var backup = file + ".bak-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        File.Copy(file, backup, true);
        var root = BinaryVdf.Parse(File.ReadAllBytes(file));
        var apps = root.GetOrCreateObject("shortcuts");
        var obj = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["appid"] = unchecked((int)(Crc32.Compute(Encoding.UTF8.GetBytes(Path.GetFileName(build.ExecutablePath) + build.Build.DisplayName)) | 0x80000000u)).ToString(),
            ["AppName"] = build.Build.DisplayName,
            ["Exe"] = $"\"{build.ExecutablePath}\"",
            ["StartDir"] = $"\"{build.StartDirectory}\"",
            ["icon"] = "",
            ["ShortcutPath"] = "",
            ["LaunchOptions"] = build.Build.DefaultArguments(options.Map, options.Height, options.Width),
            ["IsHidden"] = "0",
            ["AllowDesktopConfig"] = "1",
            ["OpenVR"] = "0",
            ["Devkit"] = "0",
            ["DevkitGameID"] = "",
            ["LastPlayTime"] = "0",
            ["FlatpakAppID"] = "",
            ["tags"] = new Dictionary<string, object> { ["0"] = "Portal 2 Beta" }
        };
        apps.AddObject(NextIndex(apps), obj);
        File.WriteAllBytes(file, root.Serialize());
    }

    private static string NextIndex(BinaryVdfObject obj) => obj.Children.Keys.Where(k => int.TryParse(k, out _)).Select(int.Parse).DefaultIfEmpty(-1).Max().PlusOne().ToString();
    private static string? LocateSteamInstall()
    {
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        foreach (var sub in new[] { "Software\\Valve\\Steam", "Software\\WOW6432Node\\Valve\\Steam" })
        {
            try { using var k = hive.OpenSubKey(sub); var p = k?.GetValue("SteamPath") as string ?? k?.GetValue("InstallPath") as string; if (!string.IsNullOrWhiteSpace(p) && Directory.Exists(p)) return p; } catch { }
        }
        return null;
    }
}

internal static class IntExtensions { public static int PlusOne(this int n) => n + 1; }

internal sealed class BinaryVdfObject
{
    public Dictionary<string, object> Children { get; } = new(StringComparer.Ordinal);
    public BinaryVdfObject GetOrCreateObject(string name) { if (Children.TryGetValue(name, out var value) && value is BinaryVdfObject o) return o; var n = new BinaryVdfObject(); Children[name] = n; return n; }
    public void AddObject(string key, Dictionary<string,object> fields) { var obj = new BinaryVdfObject(); foreach (var p in fields) { obj.Children[p.Key] = p.Value is Dictionary<string,object> d ? Make(d) : p.Value; } Children[key] = obj; static BinaryVdfObject Make(Dictionary<string,object> d) { var o = new BinaryVdfObject(); foreach (var x in d) o.Children[x.Key] = x.Value is Dictionary<string,object> nd ? Make(nd) : x.Value; return o; } }
    public byte[] Serialize() { using var ms = new MemoryStream(); using var w = new BinaryWriter(ms, Encoding.UTF8, true); WriteObject(w, this, root:true); return ms.ToArray(); }
    private static void WriteObject(BinaryWriter w, BinaryVdfObject obj, bool root=false) { foreach (var kv in obj.Children) { w.Write((byte)0); WriteString(w, kv.Key); if (kv.Value is BinaryVdfObject child) { w.Write((byte)8); WriteObject(w, child); w.Write((byte)8); } else { w.Write((byte)1); WriteString(w, kv.Value?.ToString() ?? ""); } } if (!root) { } w.Write((byte)8); }
    private static void WriteString(BinaryWriter w, string value) { var b = Encoding.UTF8.GetBytes(value + "\0"); w.Write(b); }
}

internal static class BinaryVdf
{
    public static BinaryVdfObject Parse(byte[] data)
    {
        var root = new BinaryVdfObject(); if (data.Length == 0) return root;
        using var ms = new MemoryStream(data); using var r = new BinaryReader(ms, Encoding.UTF8);
        try { while (ms.Position < ms.Length) { var type = r.ReadByte(); if (type == 8) break; var key = ReadString(r); if (type == 1) root.Children[key] = ReadString(r); else if (type == 0) { var childType = r.ReadByte(); if (childType == 8) root.Children[key] = ReadObject(r); } } } catch { return new BinaryVdfObject(); }
        return root;
    }
    private static BinaryVdfObject ReadObject(BinaryReader r) { var o = new BinaryVdfObject(); while (true) { var t = r.ReadByte(); if (t == 8) break; var k = ReadString(r); if (t == 1) o.Children[k] = ReadString(r); else if (t == 0) { var nested = r.ReadByte(); if (nested == 8) o.Children[k] = ReadObject(r); } } return o; }
    private static string ReadString(BinaryReader r) { var b = new List<byte>(); byte x; while ((x = r.ReadByte()) != 0) b.Add(x); return Encoding.UTF8.GetString(b.ToArray()); }
}

internal static class Crc32
{
    private static readonly uint[] Table = Build();
    public static uint Compute(byte[] data) { uint crc = 0xffffffffu; foreach (var b in data) crc = (crc >> 8) ^ Table[(crc ^ b) & 0xff]; return ~crc; }
    private static uint[] Build() { var t = new uint[256]; for (uint i=0; i<256; i++) { uint c=i; for (var k=0;k<8;k++) c=(c&1)!=0 ? 0xedb88320u^(c>>1) : c>>1; t[i]=c; } return t; }
}
