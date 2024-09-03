using Microsoft.Extensions.Logging;
using OpenSearch.Net;

namespace BetterAppleJobSearch.Fetcher;

public class OpenSearchClient
{
    private const string JobsIndex = "jobs";

    private readonly ILogger logger;
    private readonly OpenSearchLowLevelClient lowLevelClient;

    public OpenSearchClient(ILoggerFactory loggerFactory, LogLevel minimumLogLevel)
    {
        this.logger = loggerFactory.CreateLogger<OpenSearchClient>();

        SingleNodeConnectionPool connectionPool =
            new SingleNodeConnectionPool(
                new Uri("https://localhost:9200"));
        ConnectionConfiguration config =
            new ConnectionConfiguration(connectionPool)
                .PrettyJson(minimumLogLevel <= LogLevel.Debug);

        this.lowLevelClient = new OpenSearchLowLevelClient(config);
    }

    public async Task PingAsync()
    {
        StringResponse response = await this.lowLevelClient.PingAsync<StringResponse>();
        this.logger.LogInformation(
            "Ping response, status={HttpStatusCode}, body={Body}",
            response.HttpStatusCode,
            response.Body);
    }

    public async Task IngestJobsAsync(List<dynamic> jobs)
    {
        // The _bulk API doesn't have a very good shape (especially when autogenerating IDs)
        // so we're just doing it one at a time.
        int successCount = 0;
        for (int i = 0; i < jobs.Count; i++)
        {
            StringResponse response =
                await this.lowLevelClient.IndexAsync<StringResponse>(
                    JobsIndex,
                    PostData.Serializable(jobs[i]));
            if (response.Success)
            {
                successCount++;
            }

            this.logger.LogDebug(
                "Ingested document {Index}, status={HttpStatusCode}, body={Body}",
                i + 1,
                response.HttpStatusCode,
                response.Body);
        }

        this.logger.LogInformation(
            "Ingested {TotalDocumentCount} documents, {SucessfulDocumentCount} succeeded",
            jobs.Count,
            successCount);
    }
}
