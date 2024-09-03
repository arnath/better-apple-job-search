using BetterAppleJobSearch.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BetterAppleJobSearch.Fetcher;

public class Program
{
    public static async Task Main()
    {
        ILoggerFactory loggerFactory =
            LoggerFactory.Create((builder) =>
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Information)
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning));
        AppleJobFetcher jobFetcher = new AppleJobFetcher(loggerFactory);

        await using DbContext dbContext = new EfCoreJobRepository(loggerFactory);
        await dbContext.Database.EnsureCreatedAsync();

        await jobFetcher.FetchAsync("./locations-2024-09-01.json");
    }
}
