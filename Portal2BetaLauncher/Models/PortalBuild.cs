namespace Portal2BetaLauncher.Models;

public sealed record PortalBuild(
    string BuildNumber,
    string Date,
    string DisplayName,
    string[] StartArguments,
    string[] ExecutableNames,
    string? FixArchive,
    string Notes = "")
{
    public string DefaultArguments(string? map, int? height, int? width)
    {
        var args = StartArguments.ToList();
        if (!string.IsNullOrWhiteSpace(map)) args.Add($"+map {QuoteArg(map!)}");
        if (height is > 0) args.Add($"+h {height}");
        if (width is > 0) args.Add($"+w {width}");
        return string.Join(" ", args);
    }

    private static string QuoteArg(string value) => value.Any(char.IsWhiteSpace) ? $"\"{value.Replace("\"", "\\\"")}\"" : value;
}
