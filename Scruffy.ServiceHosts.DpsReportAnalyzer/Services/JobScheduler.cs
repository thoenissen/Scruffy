using FluentScheduler;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Services;

/// <summary>
/// Scheduling jobs
/// </summary>
public sealed class JobScheduler : SingletonLocatedServiceBase,
                                   IAsyncDisposable,
                                   IJobFactory
{
    #region Fields

    /// <summary>
    /// Scope factory
    /// </summary>
    private IServiceScopeFactory _scopeFactory;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Starting the job server
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async Task StartAsync()
    {
        await Task.Run(JobManager.Start).ConfigureAwait(false);

        // Daily
#if RELEASE
        JobManager.AddJob<DpsReportImporter>(obj => obj.ToRunEvery(1).Days().At(3, 0));
#else
        JobManager.AddJob<DpsReportImporter>(obj => obj.ToRunNow());
#endif
    }

    #endregion // Methods

    #region SingletonLocatedServiceBase

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <remarks>When this method is called all services are registered and can be resolved.  But not all singleton services may be initialized. </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task Initialize(IServiceProvider serviceProvider)
    {
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        await base.Initialize(serviceProvider)
                  .ConfigureAwait(false);

        JobManager.JobFactory = this;
        JobManager.Initialize();
    }

    #endregion // SingletonLocatedServiceBase

    #region IJobFactory

    /// <summary>
    /// Instantiate a job of the given type.
    /// </summary>
    /// <typeparam name="T">Type of the job to instantiate</typeparam>
    /// <returns>The instantiated job</returns>
    public IJob GetJobInstance<T>()
        where T : IJob
    {
        var scope = _scopeFactory.CreateScope();

        var job = scope.ServiceProvider.GetRequiredService<T>();
        if (job is IServiceScopeSupport scopeSupport)
        {
            scopeSupport.SetScope(scope);
        }

        return job;
    }

    #endregion //IJobFactory

    #region IAsyncDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns> A task that represents the asynchronous dispose operation.</returns>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await Task.Run(JobManager.StopAndBlock).ConfigureAwait(false);
    }

    #endregion // IAsyncDisposable
}