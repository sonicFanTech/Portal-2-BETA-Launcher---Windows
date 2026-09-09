using System.IO.Compression;
using System.Security;

namespace Portal2BetaLauncher.Services;

public sealed class FixService
{
    private readonly string archivePath;
    public FixService(string? baseDirectory = null) => archivePath = Path.Combine(baseDirectory ?? AppContext.BaseDirectory, "EmbeddedFixes.zip");

    public IReadOnlyList<string> GetFixNames()
    {
        if (!File.Exists(archivePath)) return Array.Empty<string>();
        using var zip = ZipFile.OpenRead(archivePath);
        return zip.Entries.Where(e => e.Name.EndsWith("Fix.zip", StringComparison.OrdinalIgnoreCase)).Select(e => e.Name).OrderBy(x => x).ToList();
    }

    public void Install(string fixArchiveName, string targetRoot)
    {
        if (!File.Exists(archivePath)) throw new FileNotFoundException("Embedded fix pack is missing.", archivePath);
        Directory.CreateDirectory(targetRoot);
        var backup = Path.Combine(targetRoot, ".p2beta-backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(backup);
        using var outer = ZipFile.OpenRead(archivePath);
        var entry = outer.Entries.FirstOrDefault(e => string.Equals(e.Name, fixArchiveName, StringComparison.OrdinalIgnoreCase));
        if (entry is null) throw new FileNotFoundException($"Fix '{fixArchiveName}' was not found in the embedded fix pack.");
        var tempZip = Path.Combine(Path.GetTempPath(), $"p2beta-{Guid.NewGuid():N}.zip");
        try
        {
            entry.ExtractToFile(tempZip, overwrite: true);
            BackupKnownFiles(tempZip, targetRoot, backup);
            ExtractZipSafely(tempZip, targetRoot);
        }
        finally { TryDelete(tempZip); }
    }

    private static void BackupKnownFiles(string zipPath, string root, string backup)
    {
        using var zip = ZipFile.OpenRead(zipPath);
        foreach (var e in zip.Entries.Where(e => !string.IsNullOrEmpty(e.Name)))
        {
            var relative = StripOuterFolder(e.FullName).Replace('/', Path.DirectorySeparatorChar);
            var destination = SafeCombine(root, relative);
            if (File.Exists(destination))
            {
                var b = Path.Combine(backup, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(b)!);
                File.Copy(destination, b, true);
            }
        }
    }

    private static void ExtractZipSafely(string zipPath, string root)
    {
        using var zip = ZipFile.OpenRead(zipPath);
        foreach (var e in zip.Entries)
        {
            var relative = StripOuterFolder(e.FullName).Replace('/', Path.DirectorySeparatorChar);
            var destination = SafeCombine(root, relative);
            if (string.IsNullOrEmpty(e.Name)) { Directory.CreateDirectory(destination); continue; }
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            e.ExtractToFile(destination, true);
        }
    }

    private static string StripOuterFolder(string entry)
    {
        entry = entry.Replace('\\', '/').TrimStart('/');
        var slash = entry.IndexOf('/');
        return slash >= 0 ? entry[(slash + 1)..] : entry;
    }

    private static string SafeCombine(string root, string relative)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var destination = Path.GetFullPath(Path.Combine(root, relative));
        if (!destination.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)) throw new SecurityException("Fix contains an unsafe archive path.");
        return destination;
    }

    private static void TryDelete(string path) { try { File.Delete(path); } catch { } }
}
