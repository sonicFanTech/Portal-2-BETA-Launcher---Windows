using System.Collections.Concurrent;
using Portal2BetaLauncher.Models;

namespace Portal2BetaLauncher.Services;

/// <summary>
/// Fast recursive scanner for Portal 2 beta executables.
/// By default every ready Windows drive is scanned. User-selected folders can be added too.
/// Traversal is parallelized through one global directory queue, so an SSD-heavy machine does
/// not create a worker pool per drive. Reparse points are deliberately not followed because
/// junctions/symlinks can form loops or cause the same volume to be scanned repeatedly.
/// </summary>
public sealed class BuildScanner
{
    public event Action<string>? Status;

    public Task<IReadOnlyList<DetectedBuild>> ScanAsync(
        IEnumerable<string>? additionalRoots = null,
        bool includeAllReadyDrives = true,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Scan(additionalRoots, includeAllReadyDrives, cancellationToken), cancellationToken);

    private IReadOnlyList<DetectedBuild> Scan(
        IEnumerable<string>? additionalRoots,
        bool includeAllReadyDrives,
        CancellationToken token)
    {
        var roots = BuildRoots(additionalRoots, includeAllReadyDrives);
        var found = new ConcurrentDictionary<string, DetectedBuild>(StringComparer.OrdinalIgnoreCase);
        var buildCatalog = BuildCatalog.All.ToArray();

        if (roots.Count == 0)
        {
            Status?.Invoke("No accessible drives or folders were found.");
            return Array.Empty<DetectedBuild>();
        }

        var directories = new ConcurrentQueue<string>(roots);
        long pending = roots.Count;
        long visited = 0;
        var workerCount = Math.Clamp(Environment.ProcessorCount, 2, 12);
        var workers = new Task[workerCount];

        Status?.Invoke($"Scanning {roots.Count} root(s) with {workerCount} workers...");

        for (var i = 0; i < workerCount; i++)
        {
            workers[i] = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (!directories.TryDequeue(out var directory))
                    {
                        if (Volatile.Read(ref pending) == 0)
                            break;

                        Thread.Yield();
                        continue;
                    }

                    try
                    {
                        var discoveredDirectories = ScanDirectoryContents(
                            directory, found, buildCatalog, token);

                        // Increment pending before publishing child directories. This prevents
                        // another worker from declaring the scan complete while this worker is
                        // between enumerating the folder and queueing its children.
                        foreach (var child in discoveredDirectories)
                        {
                            Interlocked.Increment(ref pending);
                            directories.Enqueue(child);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        // Access denied, disappearing drives, malformed directories, etc.
                        // are isolated to the affected directory instead of ending the scan.
                    }
                    finally
                    {
                        var count = Interlocked.Increment(ref visited);
                        if (count % 1000 == 0)
                            Status?.Invoke($"Checked {count:N0} folders...");

                        Interlocked.Decrement(ref pending);
                    }
                }
            }, token);
        }

        try
        {
            Task.WaitAll(workers);
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
        {
            // Cancellation is expected.
        }

        return found.Values
            .OrderBy(x => Array.IndexOf(buildCatalog, x.Build))
            .ThenBy(x => x.ExecutablePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> ScanDirectoryContents(
        string directory,
        ConcurrentDictionary<string, DetectedBuild> found,
        PortalBuild[] buildCatalog,
        CancellationToken token)
    {
        var discoveredDirectories = new List<string>();

        foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
        {
            token.ThrowIfCancellationRequested();

            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(entry);
            }
            catch
            {
                continue;
            }

            if ((attributes & FileAttributes.Directory) != 0)
            {
                // Walking ordinary directories is exhaustive. Reparse points are skipped only
                // to prevent junction/symlink loops; Windows does not expose their real target
                // as an ordinary child directory anyway.
                if ((attributes & FileAttributes.ReparsePoint) == 0)
                    discoveredDirectories.Add(entry);
                continue;
            }

            var fileName = Path.GetFileName(entry);
            if (!fileName.Equals("hl2.exe", StringComparison.OrdinalIgnoreCase) &&
                !fileName.Equals("portal2.exe", StringComparison.OrdinalIgnoreCase))
                continue;

            // Check the COMPLETE path rather than just the final few components. That avoids
            // missing a beta when someone nests it deeply, e.g. E:\Archive\Old\Portal\852_4\...
            foreach (var build in buildCatalog)
            {
                if (!entry.Contains(build.BuildNumber, StringComparison.OrdinalIgnoreCase))
                    continue;

                var key = Path.GetFullPath(entry);
                found.TryAdd(key, new DetectedBuild(
                    build,
                    Path.GetDirectoryName(entry) ?? directory,
                    entry,
                    "Build number matched in full path"));
            }
        }

        return discoveredDirectories;
    }

    private static IReadOnlyList<string> BuildRoots(IEnumerable<string>? additionalRoots, bool includeAllReadyDrives)
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (includeAllReadyDrives)
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.IsReady)
                        roots.Add(Path.GetFullPath(drive.RootDirectory.FullName));
                }
                catch
                {
                    // The drive may disappear while being queried.
                }
            }
        }

        if (additionalRoots is not null)
        {
            foreach (var root in additionalRoots)
            {
                if (string.IsNullOrWhiteSpace(root))
                    continue;

                try
                {
                    var full = Path.GetFullPath(root.Trim());
                    if (Directory.Exists(full))
                        roots.Add(full.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
                }
                catch
                {
                    // Ignore malformed or stale custom roots.
                }
            }
        }

        return roots.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
