namespace Portal2BetaLauncher.Models;

public static class BuildCatalog
{
    public static IReadOnlyList<PortalBuild> All { get; } = new[]
    {
        new PortalBuild("852_0", "July 2009", "Portal 2 Beta — 852_0", new[] { "-tempcontent", "-console", "-steam", "-windowed" }, new[] { "hl2.exe" }, "July 2010 Fix.zip", "Earliest known build in this catalog."),
        new PortalBuild("841_0", "February 2010", "Portal 2 Beta — 841_0", new[] { "-tempcontent", "-console", "-steam", "-windowed" }, new[] { "hl2.exe" }, null),
        new PortalBuild("852_1", "March 2010", "Portal 2 Beta — 852_1", new[] { "-tempcontent", "-console", "-steam", "-windowed" }, new[] { "hl2.exe" }, null),
        new PortalBuild("852_2", "July 2010", "Portal 2 Beta — 852_2", new[] { "-game", "portal2", "-tempcontent", "-console", "-windowed" }, new[] { "hl2.exe" }, "July 2010 Fix.zip"),
        new PortalBuild("852_3", "August 2010", "Portal 2 Beta — 852_3", new[] { "-game", "portal2", "-tempcontent", "-console", "-windowed" }, new[] { "hl2.exe" }, "August Fix.zip"),
        new PortalBuild("841_1", "September 24 2010", "Portal 2 Beta — 841_1", new[] { "-game", "portal2", "-tempcontent", "-console", "-windowed" }, new[] { "hl2.exe" }, "September Fix.zip"),
        new PortalBuild("841_2", "September 26 2010", "Portal 2 Beta — 841_2", new[] { "-game", "portal2", "-tempcontent", "-console", "-windowed" }, new[] { "hl2.exe" }, "September Fix.zip"),
        new PortalBuild("852_4", "October 2010", "Portal 2 Beta — 852_4", new[] { "-game", "portal2", "-tempcontent", "-console", "-windowed" }, new[] { "hl2.exe" }, "October Fix.zip"),
        new PortalBuild("852_6", "December 2010", "Portal 2 Beta — 852_6", new[] { "-console", "-steam", "-windowed" }, new[] { "portal2.exe" }, "December Fix.zip"),
        new PortalBuild("841_3", "January 2011", "Portal 2 Beta — 841_3", new[] { "-game", "portal2", "-tempcontent", "-console", "-windowed" }, new[] { "hl2.exe", "portal2.exe" }, "Jan Fix.zip", "January build may use portal2_dlc3 depending on the dump.")
    };

    public static PortalBuild? Find(string buildNumber) => All.FirstOrDefault(b => string.Equals(b.BuildNumber, buildNumber, StringComparison.OrdinalIgnoreCase));
}
