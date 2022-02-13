using FluentScheduler;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.JobScheduler;

/// <summary>
/// Asynchronous executing of a job
/// </summary>
public abstract class LocatedAsyncJob : IAsyncJob
{
    #region Fields

    /// <summary>
    /// Localization group
    /// </summary>
    private LocalizationGroup _localizationGroup;

    #endregion // Fields

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
}