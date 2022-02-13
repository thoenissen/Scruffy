using FluentScheduler;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Core;

/// <summary>
/// Batch of jobs
/// </summary>
internal abstract class BatchJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// List of jobs to be executed
    /// </summary>
    private readonly List<Type> _jobTypes;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="types">Jobs</param>
    /// <remarks>Jobs will be executed in the given order.</remarks>
    protected BatchJob(IEnumerable<Type> types)
    {
        _jobTypes = types.ToList();
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            foreach (var jobType in _jobTypes)
            {
                var scope = scopeFactory.CreateAsyncScope();
                await using (scope.ConfigureAwait(false))
                {
                    var job = scope.ServiceProvider.GetService(jobType);
                    if (job is IAsyncJob asyncJob)
                    {
                        await asyncJob.ExecuteAsync()
                                      .ConfigureAwait(false);
                    }
                    else if (job is IJob syncJob)
                    {
                        syncJob.Execute();
                    }
                    else
                    {
                        LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(BatchJob), "Invalid job type", jobType.ToString());
                    }
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}