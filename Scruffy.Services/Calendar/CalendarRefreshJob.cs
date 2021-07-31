using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Calendar
{
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
        protected override async Task ExecuteAsync()
        {
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
            {
                await serviceProvider.GetService<CalendarScheduleService>()
                                     .CreateAppointments(null)
                                     .ConfigureAwait(false);

                await serviceProvider.GetService<CalendarMessageBuilderService>()
                                     .RefreshMessages(null)
                                     .ConfigureAwait(false);
            }
        }

        #endregion // LocatedAsyncJob
    }
}
