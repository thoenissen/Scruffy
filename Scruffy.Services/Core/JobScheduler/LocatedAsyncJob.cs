using System;
using System.Threading.Tasks;

using FluentScheduler;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.JobScheduler
{
    /// <summary>
    /// Asynchronous executing of a job
    /// </summary>
    public abstract class LocatedAsyncJob : IJob
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
        public abstract Task ExecuteAsync();

        #endregion // Methods

        #region Properties

        /// <summary>
        /// Localized group
        /// </summary>
        public LocalizationGroup LocalizationGroup => _localizationGroup ??= GlobalServiceProvider.Current.GetServiceProvider().GetService<LocalizationService>().GetGroup(GetType().Name);

        #endregion // Properties

        #region IJob

        /// <summary>
        /// Executes the job
        /// </summary>
        public void Execute()
        {
            try
            {
                LoggingService.AddJobLogEntry(LogEntryLevel.Information, GetType().Name, "Job started");

                Task.Run(ExecuteAsync).Wait();
            }
            catch (Exception ex)
            {
                LoggingService.AddJobLogEntry(LogEntryLevel.CriticalError, GetType().Name, ex.Message, null, ex);
            }
        }

        #endregion // IJob
    }
}