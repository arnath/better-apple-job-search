using System.Text;
using BetterAppleJobSearch.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BetterAppleJobSearch;

public class AppleJobFetcher(ILoggerFactory loggerFactory) : IDisposable
{
    private const string AppleCsrfHeader = "X-Apple-CSRF-Token";

    private static readonly JsonSerializerSettings DefaultSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    private readonly ILogger logger = loggerFactory.CreateLogger<AppleJobFetcher>();
    private readonly HttpClient httpClient = new HttpClient();
    private string? nextCsrfToken;

    public async Task FetchAsync(string locationsJsonFilePath)
    {
        // string locationsJson = await File.ReadAllTextAsync("./Apple/locations-2024-09-01.json");
        string locationsJson = await File.ReadAllTextAsync(locationsJsonFilePath);
        JArray locations = JArray.Parse(locationsJson);
        Dictionary<string, dynamic> allJobs = new();
        int rep = 0;
        foreach (dynamic location in locations)
        {
            List<dynamic> locationJobs = await this.FetchJobsForLocationAsync(location.id.ToString());
            foreach (dynamic job in locationJobs)
            {
                string? jobId = job.id;
                if (jobId != null)
                {
                    allJobs[jobId] = job;
                }
            }
        
            this.logger.LogInformation("Location {rep} of {locationCount}, jobs so far: {jobCount}", ++rep,
                locations.Count, allJobs.Count);
        }

        await using EfCoreJobRepository jobRepository = new EfCoreJobRepository(loggerFactory);
        await jobRepository.InsertAppleJobsAsync(allJobs.Values);
    }

    public void Dispose()
    {
        this.httpClient.Dispose();
    }

    private async Task<List<dynamic>> FetchJobsForLocationAsync(string locationId)
    {
        JobSearchRequest jobSearchRequest = new JobSearchRequest(locationId);
        List<dynamic> jobs = [];
        int expectedJobCount = 1;
        while (true)
        {
            using HttpRequestMessage request =
                new HttpRequestMessage(HttpMethod.Post, "https://jobs.apple.com/api/role/search");

            string serializedRequest = JsonConvert.SerializeObject(jobSearchRequest, DefaultSerializerSettings);
            request.Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            if (this.nextCsrfToken != null)
            {
                request.Headers.Add(AppleCsrfHeader, this.nextCsrfToken);
            }
            
            this.logger.LogDebug("Sending request to https://jobs.apple.com/api/role/search, headers={headers}, body={body}",
                request.Headers.Select((h) => $"{h.Key}: {string.Join(", ", h.Value)}"),
                serializedRequest);

            using HttpResponseMessage response = await this.httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            this.nextCsrfToken = response.Headers.GetValues(AppleCsrfHeader).FirstOrDefault();

            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError("Request failed, status code={statusCode}, body={body}", response.StatusCode, responseJson);
                break;
            }
            
            dynamic result = JObject.Parse(responseJson);
            int originalJobCount = jobs.Count;
            jobs.AddRange(result.searchResults);

            logger.LogDebug("Response body from request={body}", responseJson);
            
            // Break if we failed to find any new jobs or we've reached the expected count.
            if (result.totalRecords != null)
            {
                expectedJobCount = (int)result.totalRecords;
            }
            
            if (jobs.Count == originalJobCount || jobs.Count >= expectedJobCount)
            {
                break;
            }

            // Ask for the next page.
            jobSearchRequest.Page++;
        }

        return jobs;
    }
}
