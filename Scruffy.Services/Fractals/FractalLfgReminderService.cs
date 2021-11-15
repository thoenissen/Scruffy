using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Fractals.Jobs;

namespace Scruffy.Services.Fractals;

/// <summary>
/// Management of the fractal reminders
/// </summary>
public sealed class FractalLfgReminderService : IDisposable
{
    #region Fields

    /// <summary>
    /// Job scheduler
    /// </summary>
    private JobScheduler _jobScheduler;

    /// <summary>
    /// Locking
    /// </summary>
    private LockFactory _lockFactory;

    /// <summary>
    /// Current job
    /// </summary>
    private FractalReminderJob _currentJob;

    /// <summary>
    /// Name of the current Job
    /// </summary>
    private string _currentJobName;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="jobScheduler">Job scheduler</param>
    public FractalLfgReminderService(JobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
        _lockFactory = new LockFactory();
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Creation of a job to create the next reminder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateNextReminderJobAsync()
    {
        var timeStamp = DateTime.Now.AddHours(2);

        await using (var unused = (await _lockFactory.CreateLockAsync().ConfigureAwait(false)).ConfigureAwait(false))
        {
            if (_currentJobName != null)
            {
                _jobScheduler.RemoveJob(_currentJobName);

                _currentJobName = null;
                _currentJob = null;
            }

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var next = await dbFactory.GetRepository<FractalRegistrationRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.AppointmentTimeStamp > timeStamp)
                                          .GroupBy(obj => new
                                                          {
                                                              obj.ConfigurationId,
                                                              obj.AppointmentTimeStamp
                                                          })
                                          .Select(obj => new
                                                         {
                                                             TimeStamp = obj.Key.AppointmentTimeStamp,
                                                             Count = obj.Count()
                                                         })
                                          .Where(obj => obj.Count >= 5)
                                          .Select(obj => (DateTime?)obj.TimeStamp)
                                          .OrderBy(obj => obj)
                                          .FirstOrDefaultAsync()
                                          .ConfigureAwait(false);

                if (next != null)
                {
                    _currentJob = new FractalReminderJob(next.Value);
                    _currentJobName = _jobScheduler.AddJob(_currentJob, next.Value.AddHours(-2));
                }
            }
        }
    }

    /// <summary>
    /// Creation or refresh of the job to create the next reminder
    /// </summary>
    /// <param name="timeStamp">Timestamp of the changed appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RefreshNextReminderJobAsync(DateTime timeStamp)
    {
        if (_currentJob == null
         || _currentJob.TimeStamp >= timeStamp)
        {
            await CreateNextReminderJobAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creation of a job to delete the just posted reminder
    /// </summary>
    /// <param name="channelId">Id of the discord channel</param>
    /// <param name="messageId">Id of the discord message</param>
    public void CreateReminderDeletionJob(ulong channelId, ulong messageId)
    {
        _jobScheduler.AddJob(new FractalReminderDeletionJob(channelId, messageId), DateTime.Now.AddHours(4));
    }

    /// <summary>
    /// Creates jobs to delete the reminder messages
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateReminderDeletionJobsAsync()
    {
        var now = DateTime.Now;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var appointments = await dbFactory.GetRepository<FractalAppointmentRepository>()
                                              .GetQuery()
                                              .Where(obj => obj.AppointmentTimeStamp >= now)
                                              .Select(obj => new
                                                             {
                                                                 ChannelId = obj.FractalLfgConfiguration.DiscordChannelId,
                                                                 MessageId = obj.DiscordMessageId,
                                                                 obj.AppointmentTimeStamp
                                                             })
                                              .ToListAsync()
                                              .ConfigureAwait(false);

            foreach (var appointment in appointments)
            {
                _jobScheduler.AddJob(new FractalReminderDeletionJob(appointment.ChannelId, appointment.MessageId), appointment.AppointmentTimeStamp.AddHours(2));
            }
        }
    }

    #endregion // Methods

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _lockFactory?.Dispose();
        _lockFactory = null;
    }

    #endregion // IDisposable
}