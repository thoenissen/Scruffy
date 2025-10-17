using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Reminder.Jobs;

/// <summary>
/// Execution of a one time reminder
/// </summary>
public class OneTimeReminderJob : LocatedAsyncJob
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
    /// <param name="id">Entity id</param>
    public OneTimeReminderJob(long id)
    {
        _id = id;
    }

    #endregion // Constructor

    #region IJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var transaction = dbFactory.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

            await using (transaction.ConfigureAwait(false))
            {
                var jobEntity = dbFactory.GetRepository<OneTimeReminderRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.Id == _id)
                                         .Select(obj => new
                                                        {
                                                            ChannelId = obj.DiscordChannelId,
                                                            obj.IsExecuted,
                                                            obj.DiscordAccountId,
                                                            obj.Message
                                                        })
                                         .FirstOrDefault();

                if (jobEntity?.IsExecuted == false)
                {
                    if (dbFactory.GetRepository<OneTimeReminderRepository>()
                                 .Refresh(obj => obj.Id == _id,
                                          obj => obj.IsExecuted = true))
                    {
                        await transaction.CommitAsync()
                                         .ConfigureAwait(false);

                        var isExecuted = false;

                        try
                        {
                            var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

                            await using (serviceProvider.ConfigureAwait(false))
                            {
                                var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

                                var channel = await discordClient.GetChannelAsync(jobEntity.ChannelId).ConfigureAwait(false);

                                if (channel is IMessageChannel textChannel)
                                {
                                    var user = await discordClient.GetUserAsync(jobEntity.DiscordAccountId).ConfigureAwait(false);

                                    await textChannel.SendMessageAsync(string.IsNullOrWhiteSpace(jobEntity.Message)
                                                                           ? LocalizationGroup.GetFormattedText("EmptyReminder", "{0} Reminder", user.Mention)
                                                                           : jobEntity.Message.Contains("\n")
                                                                               ? LocalizationGroup.GetFormattedText("MultiLineReminder", "{0} Reminder:\n\n{1}", user.Mention, jobEntity.Message)
                                                                               : LocalizationGroup.GetFormattedText("SingleLineReminder", "{0} Reminder: {1}", user.Mention, jobEntity.Message))
                                                     .ConfigureAwait(false);

                                    isExecuted = true;
                                }
                            }
                        }
                        finally
                        {
                            if (isExecuted == false)
                            {
                                dbFactory.GetRepository<OneTimeReminderRepository>()
                                         .Refresh(obj => obj.Id == _id,
                                                  obj => obj.IsExecuted = false);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion // IJob
}