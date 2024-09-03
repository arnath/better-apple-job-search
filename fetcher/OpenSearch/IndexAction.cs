using Newtonsoft.Json;

namespace BetterAppleJobSearch.Fetcher.OpenSearch;

public class IndexAction(string indexName, string id)
{
    [JsonProperty("index")]
    public IndexActionMetadata Metadata => new IndexActionMetadata(indexName, id);
}

public class IndexActionMetadata(string indexName, string id)
{
    [JsonProperty("_index")]
    public string IndexName => indexName;

    [JsonProperty("_id")]
    public string Id => id;
}
