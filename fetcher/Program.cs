using BetterAppleJobSearch.Fetcher.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenSearch.Net;

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

        using AppleJobFetcher jobFetcher = new AppleJobFetcher(loggerFactory);
        List<dynamic> jobs = await jobFetcher.FetchAsync("./locations-tiny.json");

        OpenSearchLowLevelClient openSearchClient = new OpenSearchLowLevelClient();
        StringResponse openSearchResponse = await openSearchClient.BulkAsync<StringResponse>(PostData.MultiJson(jobs));
        Console.WriteLine(openSearchResponse.HttpStatusCode);
        Console.WriteLine(openSearchResponse.Body);
    }
}
