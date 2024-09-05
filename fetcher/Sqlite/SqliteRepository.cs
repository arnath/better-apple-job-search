using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BetterAppleJobSearch.Fetcher.Sqlite;

public class SqliteRepository(ILoggerFactory loggerFactory) : DbContext
{
    private const string AppleOwnerName = "apple";

    public DbSet<JobEntity> Jobs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlite("Data Source=/Users/vijay/code/better-apple-job-search/ui/static/jobs-2024-09-01.db")
            .UseSnakeCaseNamingConvention()
            .UseLoggerFactory(loggerFactory);

    public async Task InsertAppleJobsAsync(IEnumerable<dynamic> appleJobs)
    {
        foreach (dynamic jobData in appleJobs)
        {
            // If this field doesn't exist, this assigns some default value to postDate.
            DateTime.TryParse(jobData.postDateInGMT?.ToString(), out DateTime postDate);

            JobEntity job = new JobEntity()
            {
                Id = $"apple_{jobData.id.ToString()}",
                Link = new Uri($"https://jobs.apple.com/en-us/details/{jobData.positionId.ToString()}"),
                PostDate = postDate,
                Title = jobData.postingTitle.ToString(),
                Summary = jobData.jobSummary.ToString(),
                TeamName = jobData.team?.teamName?.ToString(),
                IsRemote = (bool)jobData.homeOffice,
            };

            foreach (dynamic location in jobData.locations)
            {
                job.Locations.Add(
                    $"{location.name?.ToString() ?? "unknown"}, {location.countryName?.ToString() ?? "unknown"}");
            }

            this.Jobs.Add(job);
        }

        await this.SaveChangesAsync();
    }
}
