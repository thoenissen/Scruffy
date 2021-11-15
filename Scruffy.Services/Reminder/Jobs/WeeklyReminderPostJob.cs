using DSharpPlus;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Reminder.Jobs;

/// <summary>
/// Posting a weekly reminder
/// </summary>
public class WeeklyReminderPostJob : LocatedAsyncJob
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
    public WeeklyReminderPostJob(long id)
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
                var data = dbFactory.GetRepository<WeeklyReminderRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == _id)
                                    .Select(obj => new
                                                   {
                                                       ChannelId = obj.DiscordChannelId,
                                                       obj.Message
                                                   })
                                    .FirstOrDefault();

                if (data != null)
                {
                    var discordClient = serviceProvider.GetService<DiscordClient>();

                    var channel = await discordClient.GetChannelAsync(data.ChannelId).ConfigureAwait(false);

                    var message = await channel.SendMessageAsync(data.Message).ConfigureAwait(false);

                    dbFactory.GetRepository<WeeklyReminderRepository>()
                             .Refresh(obj => obj.Id == _id,
                                      obj => obj.DiscordMessageId = message.Id);
                }
            }
        }
    }

    #endregion // AsyncJob
}