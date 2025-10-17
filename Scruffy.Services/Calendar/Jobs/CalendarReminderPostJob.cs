using Discord;
using Discord.WebSocket;

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
    private readonly long _id;

    #endregion // Fields

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

    #region AsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

        await using (serviceProvider.ConfigureAwait(false))
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var channels = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                        .GetQuery()
                                        .Select(obj => obj);

                var data = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == _id)
                                    .Select(obj => new
                                                   {
                                                       ChannelId = channels.Where(obj2 => obj2.Guild.DiscordServerId == obj.CalendarAppointmentTemplate.DiscordServerId
                                                                                       && obj2.Type == GuildChannelConfigurationType.CalendarReminder)
                                                                           .Select(obj2 => obj2.DiscordChannelId)
                                                                           .FirstOrDefault(),
                                                       obj.DiscordChannelId,
                                                       obj.DiscordMessageId,
                                                       LeaderDiscordAccountId = obj.Leader
                                                                                   .DiscordAccounts
                                                                                   .Select(obj2 => (ulong?)obj2.Id)
                                                                                   .FirstOrDefault(),
                                                       obj.CalendarAppointmentTemplate.Description,
                                                       obj.CalendarAppointmentTemplate.ReminderMessage,
                                                       IsLeaderNeeded = obj.CalendarAppointmentTemplate.DiscordVoiceChannel != null
                                                   })
                                    .FirstOrDefault(obj => obj.ChannelId > 0);

                if (data?.ChannelId != null)
                {
                    var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

                    var channel = await discordClient.GetChannelAsync(data.ChannelId)
                                                     .ConfigureAwait(false);

                    if (channel is IMessageChannel messageChannel)
                    {
                        string messageText;

                        if (data.LeaderDiscordAccountId != null)
                        {
                            var leader = await discordClient.GetUserAsync(data.LeaderDiscordAccountId.Value)
                                                            .ConfigureAwait(false);

                            messageText = $"{data.ReminderMessage}{Environment.NewLine}{Environment.NewLine}{LocalizationGroup.GetFormattedText("Leader", "Appointment leader: {0}", leader?.Mention)}";
                        }
                        else if (data.IsLeaderNeeded)
                        {
                            messageText = LocalizationGroup.GetFormattedText("MissingLeader", "Hello Guild member.\n\nThe appointment '{0}' can unfortunately not take place due to missing appointment lead.", data.Description);

                            var calenderChannelId = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                                             .GetQuery()
                                                             .Where(obj => obj.Type == GuildChannelConfigurationType.CalendarOverview)
                                                             .Select(obj => obj.DiscordChannelId)
                                                             .FirstOrDefault();

                            if (calenderChannelId > 0)
                            {
                                messageText = $"{messageText}{Environment.NewLine}{Environment.NewLine}{LocalizationGroup.GetFormattedText("CalenderChannelRemark", "If you would like to take the lead on the appointment, you can sign up in {0}.", $"<#{calenderChannelId}>")}";

                                var explanationChannelId = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                                                    .GetQuery()
                                                                    .Where(obj => obj.Type == GuildChannelConfigurationType.CalendarLeadExplanation)
                                                                    .Select(obj => obj.DiscordChannelId)
                                                                    .FirstOrDefault();

                                if (explanationChannelId > 0)
                                {
                                    messageText = $"{messageText} {LocalizationGroup.GetFormattedText("CalendarLeadRemark", "For more information about the lead, see {0}.", $"<#{explanationChannelId}>")}";
                                }
                            }
                        }
                        else
                        {
                            messageText = data.ReminderMessage;
                        }

                        var sendMessage = true;

                        if (data.DiscordChannelId != null
                         && data.DiscordMessageId != null)
                        {
                            var message = await messageChannel.GetMessageAsync(data.DiscordMessageId.Value)
                                                              .ConfigureAwait(false);

                            if (message != null)
                            {
                                if (message.Content == messageText)
                                {
                                    sendMessage = false;
                                }
                                else
                                {
                                    await message.DeleteAsync()
                                                 .ConfigureAwait(false);
                                }
                            }
                        }

                        if (sendMessage)
                        {
                            var message = await messageChannel.SendMessageAsync(messageText, flags: MessageFlags.SuppressEmbeds)
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
        }
    }

    #endregion // AsyncJob
}