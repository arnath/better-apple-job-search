using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BetterAppleJobSearch.Fetcher.OpenSearch;

public class OpenSearchClient(ILoggerFactory loggerFactory)
{
    private const string JobsIndexName = "jobs";

    private readonly ILogger logger = loggerFactory.CreateLogger<OpenSearchClient>();
    private readonly HttpClient httpClient = new HttpClient();

    public async Task PingAsync()
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, "http://localhost:9200/");
        using HttpResponseMessage response = await this.httpClient.SendAsync(request);

        this.logger.LogInformation(
            "Ping response, status={HttpStatusCode}, body={Body}",
            response.StatusCode,
            await response.Content.ReadAsStringAsync());
    }

    public async Task GetDocumentCountAsync()
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:9200/{JobsIndexName}/_count");
        using HttpResponseMessage response = await this.httpClient.SendAsync(request);

        this.logger.LogInformation(
            "Count response, status={HttpStatusCode}, body={Body}",
            response.StatusCode,
            await response.Content.ReadAsStringAsync());
    }

    public async Task IngestJobsAsync(List<dynamic> jobs)
    {
        List<dynamic> commands = new List<dynamic>();
        foreach (dynamic job in jobs)
        {
            commands.Add(new IndexAction(JobsIndexName, job.id.ToString()!));
            commands.Add(job);
        }

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:9200/_bulk");
        string joinedCommands = string.Join("\n", commands.Select((c) => JsonConvert.SerializeObject(c))) + "\n";
        request.Content = new StringContent(joinedCommands, Encoding.UTF8, "application/x-ndjson");

        this.logger.LogDebug("Prepared commands for ingestion, commands={Commands}", joinedCommands);

        using HttpResponseMessage response = await this.httpClient.SendAsync(request);
        this.logger.LogDebug(
            "Ingest response, status={HttpStatusCode}, body={Body}",
            response.StatusCode,
            await response.Content.ReadAsStringAsync());
    }
}
