namespace BetterAppleJobSearch.Common;

public class BulkInsertRequest
{
    public required List<JobResource> Jobs { get; set; }
}
