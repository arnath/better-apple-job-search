using System.Text;
using BetterAppleJobSearch.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BetterAppleJobSearch.Fetcher.Apple;

public class AppleJobFetcher(ILoggerFactory loggerFactory) : IDisposable
{
    /// <summary>
    /// ORDER BIRD FOOD AND COMMENT ON MEGANS FACEBOOK POST
    /// </summary>
    private const string AppleCsrfHeader = "X-Apple-CSRF-Token";

    private readonly ILogger logger = loggerFactory.CreateLogger<AppleJobFetcher>();
    private readonly HttpClient httpClient = new HttpClient();
    private string? nextCsrfToken;

    public async Task<List<JobResource>> FetchAsync(string locationsJsonFilePath)
    {
        List<dynamic> locations = await ParseLocationsJsonAsync(locationsJsonFilePath);
        Dictionary<string, JobResource> allJobs = new Dictionary<string, JobResource>();
        for (int i = 0; i < locations.Count; i++)
        {
            List<dynamic> locationJobs = await this.FetchJobsForLocationAsync(locations[i].id.ToString());
            foreach (dynamic job in locationJobs)
            {
                string? jobId = job.id;
                if (jobId != null)
                {
                    allJobs[jobId] = CreateJobResource(job);
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

    private static JobResource CreateJobResource(dynamic jobData)
    {
        // If this field doesn't exist, this assigns a default value to postDate.
        DateTime.TryParse(jobData.postDateInGMT?.ToString(), out DateTime postDate);

        JobResource resource = new JobResource()
        {
            Id = $"apple_{jobData.id.ToString()}",
            Link = new Uri($"https://jobs.apple.com/en-us/details/{jobData.positionId.ToString()}"),
            PostDate = postDate,
            Title = jobData.postingTitle.ToString(),
            Summary = jobData.jobSummary.ToString(),
            TeamName = jobData.team?.teamName?.ToString(),
            IsRemote = (bool)jobData.homeOffice,
        };

        foreach (dynamic location in jobData.locations)
        {
            resource.Locations.Add(
                $"{location.name?.ToString() ?? "unknown"}, {location.countryName?.ToString() ?? "unknown"}");
        }

        return resource;
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

            string serializedRequest = JsonConvert.SerializeObject(jobSearchRequest);
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

    private async Task<List<dynamic>> ParseLocationsJsonAsync(string locationsJsonFilePath)
    {
        string locationsJson = await File.ReadAllTextAsync(locationsJsonFilePath);
        JArray locations = JArray.Parse(locationsJson);

        return locations.Cast<dynamic>().ToList();
    }
}
