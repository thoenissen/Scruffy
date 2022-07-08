using FluentScheduler;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Services.Calendar.Jobs;
using Scruffy.Services.Debug.Jobs;
using Scruffy.Services.Games.Jobs;
using Scruffy.Services.Guild.Jobs;
using Scruffy.Services.Raid.Jobs;
using Scruffy.Services.Reminder.Jobs;

namespace Scruffy.Services.Core.JobScheduler;

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

#if RELEASE
        // Daily
        JobManager.AddJob<CalendarRefreshJob>(obj => obj.ToRunEvery(1).Days().At(0, 0));
        JobManager.AddJob<LogOverviewJob>(obj => obj.ToRunEvery(1).Days().At(1, 0));
        JobManager.AddJob<GuildRankingBatchJob>(obj => obj.ToRunEvery(1).Days().At(0, 10));
        JobManager.AddJob<BackupJob>(obj => obj.ToRunEvery(1).Days().At(2, 0));

        // Guild
        JobManager.AddJob<GuildLogImportJob>(obj => obj.NonReentrant().ToRunEvery(20).Seconds());

        // Games
        JobManager.AddJob<CounterGameJob>(obj => obj.ToRunEvery(10).Minutes());
        JobManager.AddJob<WordChainJob>(obj => obj.ToRunEvery(10).Minutes());

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            // one tine reminders
            var oneTimeReminders = await dbFactory.GetRepository<OneTimeReminderRepository>()
                                                  .GetQuery()
                                                  .Where(obj => obj.IsExecuted == false)
                                                  .Select(obj => new
                                                                 {
                                                                     obj.Id,
                                                                     obj.TimeStamp
                                                                 })
                                                  .ToListAsync().ConfigureAwait(false);

            foreach (var oneTimeReminder in oneTimeReminders)
            {
                JobManager.AddJob(new OneTimeReminderJob(oneTimeReminder.Id), obj => obj.ToRunOnceAt(oneTimeReminder.TimeStamp));
            }

            // weekly reminders
            var weeklyReminders = await dbFactory.GetRepository<WeeklyReminderRepository>()
                                                 .GetQuery()
                                                 .Select(obj => new
                                                                {
                                                                    obj.Id,
                                                                    obj.DayOfWeek,
                                                                    obj.PostTime,
                                                                    obj.DeletionTime
                                                                })
                                                 .ToListAsync().ConfigureAwait(false);

            foreach (var weeklyReminder in weeklyReminders)
            {
                AddWeeklyReminder(weeklyReminder.Id, weeklyReminder.DayOfWeek, weeklyReminder.PostTime, weeklyReminder.DeletionTime);
            }

            // calendar reminders
            var from = DateTime.Today;
            var to = DateTime.Today.AddDays(1);

            foreach (var entry in dbFactory.GetRepository<CalendarAppointmentRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.TimeStamp > from
                                                      && obj.TimeStamp < to
                                                      && obj.CalendarAppointmentTemplate.ReminderTime != null)
                                           .Select(obj => new
                                                          {
                                                              obj.Id,
                                                              obj.CalendarAppointmentTemplate.ReminderTime,
                                                              obj.TimeStamp,
                                                              obj.DiscordMessageId
                                                          })
                                           .ToList())
            {
                if (entry.DiscordMessageId == null
                 && entry.TimeStamp > DateTime.Now
                 && entry.ReminderTime != null)
                {
                    JobManager.AddJob(new CalendarReminderPostJob(entry.Id), obj => obj.ToRunOnceAt(DateTime.Today.Add(entry.ReminderTime.Value)));
                }

                JobManager.AddJob(new CalendarReminderDeletionJob(entry.Id), obj => obj.ToRunOnceAt(entry.TimeStamp));
            }

            // raid
            foreach (var appointment in dbFactory.GetRepository<RaidAppointmentRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.IsCommitted == false)
                                                 .Select(obj => new
                                                                {
                                                                    obj.ConfigurationId,
                                                                    obj.TimeStamp,
                                                                    obj.Deadline
                                                                }))
            {
                JobManager.AddJob(new RaidMessageRefreshJob(appointment.ConfigurationId), obj => obj.ToRunOnceAt(appointment.Deadline));
                JobManager.AddJob(new RaidMessageRefreshJob(appointment.ConfigurationId), obj => obj.ToRunOnceAt(appointment.TimeStamp));
            }
        }
#endif
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

    /// <summary>
    /// Adding a weekly reminder
    /// </summary>
    /// <param name="id">Id of the reminder</param>
    /// <param name="dayOfWeek">Day of the week</param>
    /// <param name="postTime">Post time</param>
    /// <param name="deletionTime">Deletion time</param>
    public void AddWeeklyReminder(long id, DayOfWeek dayOfWeek, TimeSpan postTime, TimeSpan deletionTime)
    {
        var postTimeStamp = DateTime.Today.Add(postTime);

        while (postTimeStamp < DateTime.Now
            || postTimeStamp.DayOfWeek != dayOfWeek)
        {
            postTimeStamp = postTimeStamp.AddDays(1);
        }

        JobManager.AddJob(new WeeklyReminderPostJob(id),
                          obj => obj.ToRunOnceAt(postTimeStamp).AndEvery(7).Days().At(postTime.Hours, postTime.Minutes));

        var deletionTimeStamp = DateTime.Today.Add(deletionTime);

        while (deletionTimeStamp < DateTime.Now
            || deletionTimeStamp.DayOfWeek != dayOfWeek)
        {
            deletionTimeStamp = deletionTimeStamp.AddDays(1);
        }

        JobManager.AddJob(new WeeklyReminderDeletionJob(id),
                          obj => obj.ToRunOnceAt(deletionTimeStamp).AndEvery(7).Days().At(postTime.Hours, postTime.Minutes));
    }

    /// <summary>
    /// Add a job
    /// </summary>
    /// <param name="job">Job</param>
    /// <param name="timeStamp">Time stamp to run the job</param>
    /// <returns>Name of the added job</returns>
    public string AddJob(IJob job, DateTime timeStamp)
    {
        var jobName = Guid.NewGuid().ToString();

        JobManager.AddJob(job, obj => obj.WithName(jobName).ToRunOnceAt(timeStamp));

        return jobName;
    }

    /// <summary>
    /// Removes the job by the given name
    /// </summary>
    /// <param name="jobName">Name of the job</param>
    public void RemoveJob(string jobName)
    {
        JobManager.RemoveJob(jobName);
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