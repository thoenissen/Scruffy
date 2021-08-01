using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Calendar
{
    /// <summary>
    /// Posting a weekly reminder
    /// </summary>
    public class CalendarReminderPostJob : LocatedAsyncJob
    {
        #region Fields

        /// <summary>
        /// Id of the reminder
        /// </summary>
        private long _id;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        public CalendarReminderPostJob(long id)
        {
            _id = id;
        }

        #endregion // Constructor

        #region  AsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync()
        {
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var guilds = dbFactory.GetRepository<GuildRepository>()
                                          .GetQuery()
                                          .Select(obj => obj);

                    var data = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.Id == _id
                                                   && obj.ReminderMessageId == null)
                                        .Select(obj => new
                                                       {
                                                           ChannelId = guilds.Where(obj2 => obj2.DiscordServerId == obj.CalendarAppointmentTemplate.ServerId)
                                                                                            .Select(obj2 => obj2.ReminderChannelId)
                                                                                            .FirstOrDefault(),
                                                           obj.CalendarAppointmentTemplate.ReminderMessage
                                                       })
                                        .FirstOrDefault(obj => obj.ChannelId != null);

                    if (data?.ChannelId != null)
                    {
                        var discordClient = serviceProvider.GetService<DiscordClient>();

                        var channel = await discordClient.GetChannelAsync(data.ChannelId.Value)
                                                         .ConfigureAwait(false);

                        var message = await channel.SendMessageAsync(data.ReminderMessage)
                                                   .ConfigureAwait(false);

                        dbFactory.GetRepository<CalendarAppointmentRepository>()
                                 .Refresh(obj => obj.Id == _id,
                                          obj =>
                                          {
                                              obj.ReminderChannelId = data.ChannelId;
                                              obj.ReminderMessageId = message.Id;
                                          });
                    }
                }
            }
        }

        #endregion // AsyncJob
    }
}
