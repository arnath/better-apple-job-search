namespace BetterAppleJobSearch.Common;

public class BulkInsertRequest
{
    public BulkInsertRequest(List<JobResource> jobs)
    {
        this.Jobs = jobs;
    }

    public List<JobResource> Jobs { get; set; }
}
