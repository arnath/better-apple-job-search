using BetterAppleJobSearch.Backend.Handlers;
using Microsoft.Extensions.Logging;
using Sydney.Core;

namespace BetterAppleJobSearch.Backend;

public class Program
{
    internal static readonly ILoggerFactory LoggerFactory =
        Microsoft.Extensions.Logging.LoggerFactory.Create((builder) =>
            builder
                .AddConsole()
                .SetMinimumLevel(MinimumLogLevel)
                // EF Core logs these at Information by default
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning));

    private static readonly LogLevel MinimumLogLevel = LogLevel.Information;

    public static async Task Main()
    {
        SydneyServiceConfig config = new SydneyServiceConfig(
            8080,
            returnExceptionMessagesInResponse: true);

        using (SydneyService service = new SydneyService(config, LoggerFactory))
        {
            service.AddResourceHandler("/jobs", new JobsResourceHandler());
            service.AddRestHandler("/jobs/bulk-insert", new BulkInsertHandler());

            await service.StartAsync();
        }
    }
}
