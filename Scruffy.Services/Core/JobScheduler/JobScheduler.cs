using System;
using System.Threading.Tasks;

using FluentScheduler;

using Scruffy.Services.Reminder;

namespace Scruffy.Services.Core.JobScheduler
{
    /// <summary>
    /// Scheduling jobs
    /// </summary>
    public class JobScheduler : IAsyncDisposable
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public JobScheduler()
        {
            JobManager.Initialize();
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Starting the job server
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public async Task StartAsync()
        {
            await Task.Run(JobManager.Start);
        }

        /// <summary>
        /// Adding a one time reminder
        /// </summary>
        /// <param name="timeStamp">Timestamp</param>
        /// <param name="id">Database id of the entity</param>
        public void AddOneTimeReminder(DateTime timeStamp, long id)
        {
            JobManager.AddJob(new OneTimeReminderJob(id), obj => obj.ToRunOnceAt(timeStamp));
        }

        #endregion // Methods

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns> A task that represents the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await Task.Run(JobManager.StopAndBlock);
        }

        #endregion // IAsyncDisposable
    }
}
