
using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Fractals.Jobs;

/// <summary>
/// Deletion of the fractal reminder
/// </summary>
public class FractalReminderDeletionJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Channel id
    /// </summary>
    private readonly ulong _channelId;

    /// <summary>
    /// Message id
    /// </summary>
    private readonly ulong _messageId;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="channelId">Channel id</param>
    /// <param name="messageId">Message id</param>
    public FractalReminderDeletionJob(ulong channelId, ulong messageId)
    {
        _channelId = channelId;
        _messageId = messageId;
    }

    #endregion // Constructor

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
            var discordClient = serviceProvider.GetService<DiscordSocketClient>();
            if (discordClient != null)
            {
                var channel = await discordClient.GetChannelAsync(_channelId)
                                                 .ConfigureAwait(false);
                if (channel is ITextChannel textChannel)
                {
                    var message = await textChannel.GetMessageAsync(_messageId)
                                                   .ConfigureAwait(false);
                    if (message != null)
                    {
                        await message.DeleteAsync()
                                     .ConfigureAwait(false);
                    }
                }
            }
        }
    }

    #endregion LocatedAsyncJob
}