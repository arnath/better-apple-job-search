using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BetterAppleJobSearch.Fetcher.Data;

public class EfCoreJobRepository(ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<AppleJobPosting> AppleJobs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlite("Data Source=./apple-jobs.db")
            .UseLoggerFactory(loggerFactory);

    public async Task InsertAppleJobsAsync(IEnumerable<dynamic> jobs)
    {
        foreach (dynamic jobData in jobs)
        {
            AppleJobPosting job = new AppleJobPosting()
            {
                ReqId = jobData.reqId.ToString(),
                PositionId = jobData.positionId.ToString(),
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

            this.AppleJobs.Add(job);
        }

        await this.SaveChangesAsync();
    }
}
