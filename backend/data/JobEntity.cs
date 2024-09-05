using BetterAppleJobSearch.Common;

namespace BetterAppleJobSearch.Backend.Data;

public class JobEntity
{
    public string? Id { get; set; }

    public Uri? Link { get; set; }

    public required DateTime PostDate { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public string? TeamName { get; set; }

    public List<string> Locations { get; set; } = [];

    public required bool IsRemote { get; set; }

    public static JobEntity FromResource(JobResource resource)
    {
        return new JobEntity
        {
            Id = resource.Id,
            Link = resource.Link,
            PostDate = resource.PostDate,
            Title = resource.Title,
            Summary = resource.Summary,
            TeamName = resource.TeamName,
            Locations = resource.Locations,
            IsRemote = resource.IsRemote,
        };
    }

    public JobResource ToResource()
    {
        return new JobResource
        {
            Id = this.Id,
            Link = this.Link,
            PostDate = this.PostDate,
            Title = this.Title,
            Summary = this.Summary,
            TeamName = this.TeamName,
            Locations = this.Locations,
            IsRemote = this.IsRemote,
        };
    }
}
