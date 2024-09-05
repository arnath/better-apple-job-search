using Microsoft.Extensions.Logging;
using Sydney.Core;
using BetterAppleJobSearch.Common;
using BetterAppleJobSearch.Backend.Data;
using System.Net;

namespace BetterAppleJobSearch.Backend.Handlers;

public class BulkInsertHandler : RestHandlerBase
{
    public BulkInsertHandler() : base(Program.LoggerFactory)
    {
    }

    public override async Task<SydneyResponse> PostAsync(ISydneyRequest request)
    {
        using SqliteJobRepository repository = new SqliteJobRepository();

        BulkInsertRequest requestBody = await request.DeserializeJsonAsync<BulkInsertRequest>();
        foreach (JobResource resource in requestBody.Jobs)
        {
            JobEntity entity = JobEntity.FromResource(resource);
            repository.Add(entity);
        }

        await repository.SaveChangesAsync();

        return new SydneyResponse(HttpStatusCode.OK);
    }
}
