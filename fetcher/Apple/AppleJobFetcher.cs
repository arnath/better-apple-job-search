using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BetterAppleJobSearch.Fetcher.Apple;

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

    public async Task<List<dynamic>> FetchAsync(string locationsJsonFilePath)
    {
        List<dynamic> locations = await ParseLocationsJsonAsync(locationsJsonFilePath);
        Dictionary<string, dynamic> allJobs = new Dictionary<string, dynamic>();
        for (int i = 0; i < locations.Count; i++)
        {
            List<dynamic> locationJobs = await this.FetchJobsForLocationAsync(locations[i].id.ToString());
            foreach (dynamic job in locationJobs)
            {
                string? jobId = job.id;
                if (jobId != null)
                {
                    allJobs[jobId] = job;
                }
            }

            this.logger.LogInformation(
                "Location {rep} of {locationCount}, jobs so far: {jobCount}",
                i + 1,
                locations.Count,
                allJobs.Count);
        }

        return allJobs.Values.ToList();
    }

    public void Dispose()
    {
        this.httpClient.Dispose();
    }

    private async Task<List<dynamic>> ParseLocationsJsonAsync(string locationsJsonFilePath)
    {
        string locationsJson = await File.ReadAllTextAsync(locationsJsonFilePath);
        JArray locations = JArray.Parse(locationsJson);

        return locations.Cast<dynamic>().ToList();
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
            if (!response.IsSuccessStatusCode || !response.Headers.TryGetValues(AppleCsrfHeader, out var headerValues))
            {
                this.logger.LogError("Request failed, status code={statusCode}, body={body}", response.StatusCode, responseJson);
                break;
            }

            this.nextCsrfToken = headerValues?.FirstOrDefault();
            dynamic result = JObject.Parse(responseJson);
            int originalJobCount = jobs.Count;
            jobs.AddRange(result.searchResults);

            logger.LogDebug("Response body from request={body}", responseJson);

            if (result.totalRecords != null)
            {
                expectedJobCount = (int)result.totalRecords;
            }

            // Break if we failed to find any new jobs or we've reached the expected count.
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
