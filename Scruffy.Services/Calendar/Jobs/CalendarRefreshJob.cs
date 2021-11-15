using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Calendar.Jobs;

/// <summary>
/// Refreshing the calendars
/// </summary>
public class CalendarRefreshJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteAsync()
    {
        var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            await serviceProvider.GetService<CalendarScheduleService>()
                                 .CreateAppointments(null)
                                 .ConfigureAwait(false);

            await serviceProvider.GetService<CalendarMessageBuilderService>()
                                 .RefreshMessages(null)
                                 .ConfigureAwait(false);

            await serviceProvider.GetService<CalendarMessageBuilderService>()
                                 .RefreshMotds(null)
                                 .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var from = DateTime.Today;
                var to = DateTime.Today.AddDays(1);

                var jobScheduler = serviceProvider.GetService<JobScheduler>();

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
                                                              })
                                               .ToList())
                {
                    jobScheduler.AddJob(new CalendarReminderPostJob(entry.Id), DateTime.Today.Add(entry.ReminderTime.Value));
                    jobScheduler.AddJob(new CalendarReminderDeletionJob(entry.Id), entry.TimeStamp);
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}