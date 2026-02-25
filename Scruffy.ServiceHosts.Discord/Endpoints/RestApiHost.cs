using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Raid.Jobs;

namespace Scruffy.ServiceHosts.Discord.Endpoints;

/// <summary>
/// Hosts the REST API for the Discord service host
/// </summary>
internal static class RestApiHost
{
    #region Methods

    /// <summary>
    /// Creates and configures the web application for the REST API
    /// </summary>
    /// <returns>The configured web application</returns>
    public static WebApplication Create()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();

        app.MapPost("/api/raid/refresh", OnRefreshRaid);

        return app;
    }

    /// <summary>
    /// A handler for the POST request to schedule raid message refresh jobs
    /// </summary>
    /// <param name="request">Request data</param>
    /// <returns>Result</returns>
    private static IResult OnRefreshRaid(RaidMessageRefreshRequest request)
    {
        LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(RestApiHost), "Scheduling raid message refresh jobs", $"ConfigurationId: {request.ConfigurationId}, Deadline: {request.Deadline}, TimeStamp: {request.TimeStamp}");

        using (var scope = ServiceProviderContainer.Current.CreateScope())
        {
            var jobScheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();

            jobScheduler.AddJob(new RaidMessageRefreshJob(request.ConfigurationId), request.Deadline);
            jobScheduler.AddJob(new RaidMessageRefreshJob(request.ConfigurationId), request.TimeStamp);

            return Results.Accepted();
        }
    }

    #endregion // Methods
}