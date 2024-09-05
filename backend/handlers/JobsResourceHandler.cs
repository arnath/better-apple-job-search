using System.Net;
using BetterAppleJobSearch.Backend.Data;
using Microsoft.EntityFrameworkCore;
using Sydney.Core;

namespace BetterAppleJobSearch.Backend.Handlers;

public class JobsResourceHandler : ResourceHandlerBase
{
    public JobsResourceHandler() : base(Program.LoggerFactory)
    {
    }

    public override async Task<SydneyResponse> ListAsync(ISydneyRequest request)
    {
        using SqliteJobRepository repository = new SqliteJobRepository();

        List<JobEntity> entities = await repository.Jobs.ToListAsync();

        return new SydneyResponse(
            HttpStatusCode.OK,
            entities.Select((e) => e.ToResource()).ToList());
    }
}
