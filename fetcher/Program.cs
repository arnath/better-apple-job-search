using System.Net.Mime;
using System.Text;
using BetterAppleJobSearch.Common;
using BetterAppleJobSearch.Fetcher.Apple;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BetterAppleJobSearch.Fetcher;

public class Program
{
    private static readonly LogLevel MinimumLogLevel = LogLevel.Information;

    public static async Task Main()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        ILoggerFactory loggerFactory =
            LoggerFactory.Create((builder) =>
                builder
                    .AddConsole()
                    .SetMinimumLevel(MinimumLogLevel)
                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning));
        ILogger logger = loggerFactory.CreateLogger<Program>();

        using AppleJobFetcher jobFetcher = new AppleJobFetcher(loggerFactory);
        List<JobResource> jobs = await jobFetcher.FetchAsync("./locations-2024-09-01.json");

        BulkInsertRequest bulkInsertRequest = new BulkInsertRequest(jobs);
        using HttpClient httpClient = new HttpClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/jobs/bulk-insert");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(bulkInsertRequest),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        string responseJson = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Request failed, status code={statusCode}, body={body}", response.StatusCode, responseJson);
            return;
        }

        logger.LogInformation("Successfully wrote jobs to backend.");
    }
}
