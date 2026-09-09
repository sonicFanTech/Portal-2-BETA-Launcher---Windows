namespace Portal2BetaLauncher.Services;

/// <summary>
/// Persists user-added beta folders so users do not have to re-add them every launch.
/// One normalized directory path is stored per line in the user's application-data folder.
/// </summary>
public sealed class ScanRootsStore
{
    private readonly string filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Portal2BetaLauncher",
        "scan-folders.txt");

    public IReadOnlyList<string> Load()
    {
        try
        {
            if (!File.Exists(filePath))
                return Array.Empty<string>();

            return File.ReadAllLines(filePath)
                .Select(Normalize)
                .Where(p => p is not null && Directory.Exists(p!))
                .Select(p => p!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public void Save(IEnumerable<string> paths)
    {
        var normalized = paths
            .Select(Normalize)
            .Where(p => p is not null)
            .Select(p => p!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllLines(filePath, normalized);
        }
        catch (Exception ex)
        {
            throw new IOException("Could not save the Portal 2 beta scan-folder list.", ex);
        }
    }

    private static string? Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            var full = Path.GetFullPath(path.Trim());
            return full.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }
        catch
        {
            return null;
        }
    }
}
