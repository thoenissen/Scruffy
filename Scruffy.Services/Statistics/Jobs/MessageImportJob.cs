using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Services.Statistics;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Statistics.Jobs;

/// <summary>
/// Import discord messages
/// </summary>
public class MessageImportJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Import data
    /// </summary>
    private List<DiscordMessageBulkInsertData> _importData;

    /// <summary>
    /// Last imports
    /// </summary>
    private List<LastMessageImportData> _lastImports;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Get messages
    /// </summary>
    /// <param name="messageChannel">Channel</param>
    /// <param name="serverId">Server ID</param>
    /// <param name="channelId">Channel ID</param>
    /// <param name="threadId">Thread ID</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task GetMessages(IMessageChannel messageChannel, ulong serverId, ulong channelId, ulong threadId)
    {
        var lastImport = _lastImports.FirstOrDefault(obj => obj.ServerId == serverId
                                                            && obj.ChannelId == channelId
                                                            && obj.ThreadId == threadId)
                                     ?.TimeStamp.AddHours(-1);
        ulong lastMessage = 0;

        await foreach (var collection in messageChannel.GetMessagesAsync().ConfigureAwait(false))
        {
            foreach (var message in collection.OfType<IUserMessage>())
            {
                if (message.Timestamp.LocalDateTime <= lastImport)
                {
                    lastMessage = 0;

                    break;
                }

                _importData.Add(new DiscordMessageBulkInsertData
                                {
                                    ServerId = serverId,
                                    ChannelId = channelId,
                                    ThreadId = threadId,
                                    UserId = message.Author.Id,
                                    MessageId = message.Id,
                                    TimeStamp = message.Timestamp.LocalDateTime
                                });

                lastMessage = message.Id;
            }
        }

        while (lastMessage != 0)
        {
            var fromMessageId = lastMessage;

            lastMessage = 0;

            await foreach (var collection in messageChannel.GetMessagesAsync(fromMessageId, Direction.Before).ConfigureAwait(false))
            {
                foreach (var message in collection.OfType<IUserMessage>())
                {
                    if (message.Timestamp.LocalDateTime <= lastImport)
                    {
                        lastMessage = 0;

                        break;
                    }

                    _importData.Add(new DiscordMessageBulkInsertData
                                    {
                                        ServerId = serverId,
                                        ChannelId = channelId,
                                        ThreadId = threadId,
                                        UserId = message.Author.Id,
                                        MessageId = message.Id,
                                        TimeStamp = message.Timestamp.LocalDateTime
                                    });

                    lastMessage = message.Id;
                }
            }
        }
    }

    #endregion // Methods

    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

        await using (serviceProvider.ConfigureAwait(false))
        {
            var client = serviceProvider.GetService<DiscordSocketClient>();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                _lastImports = dbFactory.GetRepository<DiscordMessageRepository>()
                                           .GetQuery()
                                           .GroupBy(obj => new
                                                           {
                                                               ServerId = obj.DiscordServerId,
                                                               ChannelId = obj.DiscordChannelId,
                                                               ThreadId = obj.DiscordThreadId
                                                           })
                                           .Select(obj => new LastMessageImportData
                                                          {
                                                              ServerId = obj.Key.ServerId,
                                                              ChannelId = obj.Key.ChannelId,
                                                              ThreadId = obj.Key.ThreadId,
                                                              TimeStamp = obj.Max(obj2 => obj2.TimeStamp)
                                                          })
                                           .ToList();

                var guildIds = dbFactory.GetRepository<GuildRepository>()
                                        .GetQuery()
                                        .Select(obj => obj.DiscordServerId)
                                        .ToList();

                foreach (var guildId in guildIds)
                {
                    var guild = client.GetGuild(guildId);

                    foreach (var category in guild.CategoryChannels.Skip(4))
                    {
                        foreach (var channel in category.Channels)
                        {
                            _importData = [];

                            if (channel is SocketVoiceChannel)
                            {
                                continue;
                            }

                            if (channel is SocketForumChannel forumChannel)
                            {
                                foreach (var thread in await forumChannel.GetActiveThreadsAsync().ConfigureAwait(false))
                                {
                                    await GetMessages(thread, guild.Id, channel.Id, thread.Id).ConfigureAwait(false);
                                }

                                foreach (var thread in await forumChannel.GetPublicArchivedThreadsAsync().ConfigureAwait(false))
                                {
                                    await GetMessages(thread, guild.Id, channel.Id, thread.Id).ConfigureAwait(false);
                                }
                            }
                            else if (channel is SocketTextChannel textChannel)
                            {
                                foreach (var thread in textChannel.Threads)
                                {
                                    await GetMessages(thread, guild.Id, channel.Id, thread.Id).ConfigureAwait(false);
                                }

                                await GetMessages(textChannel, guild.Id, channel.Id, 0).ConfigureAwait(false);
                            }

                            if (await dbFactory.GetRepository<DiscordMessageRepository>()
                                               .BulkInsert(_importData)
                                               .ConfigureAwait(false) == false)
                            {
                                LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(MessageImportJob), "BulkInsert", dbFactory.LastError?.Message, dbFactory.LastError?.ToString());
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}