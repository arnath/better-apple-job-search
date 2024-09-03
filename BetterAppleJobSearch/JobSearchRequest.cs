using Newtonsoft.Json;

namespace BetterAppleJobSearch;

internal class JobSearchRequest
{
    internal JobSearchRequest(string locationId)
    {
        this.Filters = new JobSearchRequestFilters(locationId);
        this.Query = "";
        this.Locale = "en-us";
        this.Page = 1;
    }

    public JobSearchRequestFilters Filters { get; }

    public string Query { get; }
    
    public string Locale { get; }

    public int Page { get; set; }
}

internal class JobSearchRequestFilters
{
    internal JobSearchRequestFilters(string locationId)
    {
        this.PostingPostLocation = [ locationId ];
    }

    [JsonProperty("postingpostLocation")]
    public List<string> PostingPostLocation { get; }
}