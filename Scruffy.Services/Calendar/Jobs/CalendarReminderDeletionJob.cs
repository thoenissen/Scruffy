﻿using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Calendar.Jobs;

/// <summary>
/// Deletion of a weekly reminder
/// </summary>
public class CalendarReminderDeletionJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Id of the reminder
    /// </summary>
    private long _id;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">Id</param>
    public CalendarReminderDeletionJob(long id)
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
                var data = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == _id)
                                    .Select(obj => new
                                                   {
                                                       ReminderChannelId = obj.DiscordChannelId,
                                                       ReminderMessageId = obj.DiscordMessageId
                                                   })
                                    .FirstOrDefault();

                if (data?.ReminderChannelId != null
                 && data.ReminderMessageId != null)
                {
                    var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

                    var channel = await discordClient.GetChannelAsync(data.ReminderChannelId.Value)
                                                     .ConfigureAwait(false);

                    if (channel is ITextChannel textChannel)
                    {
                        var message = await textChannel.GetMessageAsync(data.ReminderMessageId.Value)
                                                       .ConfigureAwait(false);

                        await textChannel.DeleteMessageAsync(message)
                                         .ConfigureAwait(false);

                        dbFactory.GetRepository<CalendarAppointmentRepository>()
                                 .Refresh(obj => obj.Id == _id,
                                          obj =>
                                          {
                                              obj.DiscordChannelId = null;
                                              obj.DiscordMessageId = null;
                                          });
                    }
                }
            }
        }
    }

    #endregion // AsyncJob
}