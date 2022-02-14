using FluentScheduler;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.JobScheduler;

/// <summary>
/// Asynchronous executing of a job
/// </summary>
public abstract class LocatedAsyncJob : IServiceScopeSupport, IAsyncJob, IDisposable
{
    #region Fields

    /// <summary>
    /// Localization group
    /// </summary>
    private LocalizationGroup _localizationGroup;

    /// <summary>
    /// Scope
    /// </summary>
    private IServiceScope _scope;

    #endregion // Fields

    #region Finalizer

    /// <summary>
    /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
    /// </summary>
    ~LocatedAsyncJob()
    {
        Dispose(false);
    }

    #endregion // Finalizer

    #region Methods

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public abstract Task ExecuteOverrideAsync();

    #endregion // Methods

    #region Properties

    /// <summary>
    /// Localized group
    /// </summary>
    public LocalizationGroup LocalizationGroup => _localizationGroup ??= ServiceProviderContainer.Current.GetServiceProvider().GetRequiredService<LocalizationService>().GetGroup(GetType().Name);

    #endregion // Properties

    #region IJob

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        try
        {
            LoggingService.AddJobLogEntry(LogEntryLevel.Information, GetType().Name, "Job started");

            await ExecuteOverrideAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, GetType().Name, ex.Message, null, ex);
        }
    }

    /// <summary>
    /// Executes the job
    /// </summary>
    public void Execute()
    {
        try
        {
            LoggingService.AddJobLogEntry(LogEntryLevel.Information, GetType().Name, "Job started");

            Task.Run(ExecuteOverrideAsync).Wait();
        }
        catch (Exception ex)
        {
            LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, GetType().Name, ex.Message, null, ex);
        }
    }

    #endregion // IJob

    #region IServiceScopeSupport

    /// <summary>
    /// Set the current scope
    /// </summary>
    /// <param name="scope">scope</param>
    public void SetScope(IServiceScope scope)
    {
        _scope = scope;
    }

    #endregion // IServiceScopeSupport

    #region IDisposable

    /// <summary>
    /// Internal IDisposable implementation
    /// </summary>
    /// <param name="disposing">Called from <see cref="Dispose()"/></param>?
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _scope?.Dispose();
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    #endregion // IDisposable
}