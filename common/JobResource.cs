namespace BetterAppleJobSearch.Common;

/// <summary>
/// This class happens to be identical to JobEntity for now. Keeping it because
/// that may not always be the case.
/// </summary>
public class JobResource
{
    public string? Id { get; set; }

    public Uri? Link { get; set; }

    public required DateTime PostDate { get; set; }

    public required string Title { get; set; }

    public required string Summary  { get; set; }

    public string? TeamName { get; set; }

    public List<string> Locations { get; set; } = [];

    public required bool IsRemote { get; set; }
}
