namespace BetterAppleJobSearch.Fetcher.Sqlite;

public class JobEntity
{
    public required string Id { get; set; }

    public Uri? Link { get; set; }

    public required DateTime PostDate { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public string? TeamName { get; set; }

    public List<string> Locations { get; set; } = [];

    public required bool IsRemote { get; set; }
}
