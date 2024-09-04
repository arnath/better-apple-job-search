using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BetterAppleJobSearch.Fetcher.Sqlite;

public class Repository(ILoggerFactory loggerFactory) : DbContext
{

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlite($"Data Source=/Users/vijay/code/better-apple-job-search/jobs.db")
            .UseLoggerFactory(loggerFactory);
}
