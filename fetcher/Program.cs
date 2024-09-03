﻿using Microsoft.Extensions.Logging;
using OpenSearch.Net;

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

        using AppleJobFetcher jobFetcher = new AppleJobFetcher(loggerFactory);
        List<dynamic> jobs = await jobFetcher.FetchAsync("./locations-tiny.json");

        OpenSearchClient openSearchClient = new OpenSearchClient(loggerFactory, MinimumLogLevel);
        await openSearchClient.PingAsync();
        // await openSearchClient.IngestJobsAsync(jobs);
    }
}
