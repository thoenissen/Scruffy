
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Calendar.Jobs;

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
    public override async Task ExecuteAsync()
    {
        var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var channels = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                        .GetQuery()
                                        .Select(obj => obj);

                var data = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == _id
                                               && obj.DiscordMessageId == null)
                                    .Select(obj => new
                                                   {
                                                       ChannelId = channels.Where(obj2 => obj2.Guild.DiscordServerId == obj.CalendarAppointmentTemplate.DiscordServerId
                                                                                       && obj2.Type == GuildChannelConfigurationType.CalendarReminder)
                                                                           .Select(obj2 => obj2.DiscordChannelId)
                                                                           .FirstOrDefault(),
                                                       obj.CalendarAppointmentTemplate.ReminderMessage
                                                   })
                                    .FirstOrDefault(obj => obj.ChannelId > 0);

                if (data?.ChannelId != null)
                {
                    var discordClient = serviceProvider.GetService<DiscordClient>();

                    var channel = await discordClient.GetChannelAsync(data.ChannelId)
                                                     .ConfigureAwait(false);

                    var message = await channel.SendMessageAsync(data.ReminderMessage)
                                               .ConfigureAwait(false);

                    dbFactory.GetRepository<CalendarAppointmentRepository>()
                             .Refresh(obj => obj.Id == _id,
                                      obj =>
                                      {
                                          obj.DiscordChannelId = data.ChannelId;
                                          obj.DiscordMessageId = message.Id;
                                      });
                }
            }
        }
    }

    #endregion // AsyncJob
}