using System.Threading.Tasks;

using FluentScheduler;

using Microsoft.Extensions.DependencyInjection;

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
            Task.Run(ExecuteAsync).Wait();
        }

        #endregion // IJob
    }
}