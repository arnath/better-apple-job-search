using BetterAppleJobSearch.Fetcher.Apple;
using BetterAppleJobSearch.Fetcher.OpenSearch;
using BetterAppleJobSearch.Fetcher.Sqlite;
using Microsoft.Extensions.Logging;

namespace BetterAppleJobSearch.Fetcher;

public class Program
{
    private static readonly LogLevel MinimumLogLevel = LogLevel.Information;

    public static async Task Main()
    {
        ILoggerFactory loggerFactory =
            LoggerFactory.Create((builder) =>
                builder
                    .AddConsole()
                    .SetMinimumLevel(MinimumLogLevel)
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning));
        ILogger logger = loggerFactory.CreateLogger<Program>();

        using SqliteRepository repository = new SqliteRepository(loggerFactory);
        await repository.Database.EnsureDeletedAsync();
        await repository.Database.EnsureCreatedAsync();

        using AppleJobFetcher jobFetcher = new AppleJobFetcher(loggerFactory);
        List<dynamic> jobs = await jobFetcher.FetchAsync("./locations-2024-09-01.json");

        await repository.InsertAppleJobsAsync(jobs);

        logger.LogInformation("Done");
    }
}
