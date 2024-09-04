using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BetterAppleJobSearch.Fetcher.Sqlite;

public class SqliteRepository(ILoggerFactory loggerFactory) : DbContext
{
    private const string AppleOwnerName = "apple";

    public DbSet<JobEntity> Jobs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlite("Data Source=/Users/vijay/code/better-apple-job-search/db/jobs.db")
            .UseLoggerFactory(loggerFactory);

    public async Task InsertAppleJobsAsync(IEnumerable<dynamic> appleJobs)
    {
        foreach (dynamic jobData in appleJobs)
        {
            JobEntity job = new JobEntity()
            {
                Id = $"apple_{jobData.id.ToString()}",
                Link = new Uri($"https://jobs.apple.com/en-us/details/{jobData.positionId.ToString()}"),
                PostDate = jobData.postDateInGMT as DateTime? ?? DateTime.MinValue,
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
