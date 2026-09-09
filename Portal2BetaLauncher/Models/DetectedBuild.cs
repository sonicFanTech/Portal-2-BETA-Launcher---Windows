namespace Portal2BetaLauncher.Models;

public sealed record DetectedBuild(
    PortalBuild Build,
    string RootPath,
    string ExecutablePath,
    string DetectionReason)
{
    public string StartDirectory => Path.GetDirectoryName(ExecutablePath) ?? RootPath;
}
