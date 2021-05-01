using System.Threading.Tasks;

using FluentScheduler;

namespace Scruffy.Services.Core.JobScheduler
{
    /// <summary>
    /// Asynchronous executing of a job
    /// </summary>
    public abstract class AsyncJob : IJob
    {
        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task ExecuteAsync();

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