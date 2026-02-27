using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Entity.Tables.Statistics;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Services.Statistics;
using Scruffy.Services.Core;
using Scruffy.Services.Statistics.Data;

namespace Scruffy.Services.Statistics;

/// <summary>
/// Message import
/// </summary>
public sealed class MessageCollectorService : SingletonLocatedServiceBase, IDisposable
{
    #region Fields

    /// <summary>
    /// Lock factory
    /// </summary>
    private readonly LockFactory _lockFactory = new();

    /// <summary>
    /// Discord client
    /// </summary>
    private DiscordSocketClient _client;

    /// <summary>
    /// Import data
    /// </summary>
    private List<DiscordMessageBulkInsertData> _importData;

    /// <summary>
    /// Last imports
    /// </summary>
    private List<LastMessageImportData> _lastImports;

    /// <summary>
    /// Import limit
    /// </summary>
    private DateTime? _importLimit;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Start collecting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task StartCollecting()
    {
        await using (await _lockFactory.CreateLockAsync().ConfigureAwait(false))
        {
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
            }

            _client.MessageReceived += OnMessageReceived;
            _client.Disconnected += OnDisconnected;
            _client.Connected += OnConnected;

            if (_client.ConnectionState == ConnectionState.Connected)
            {
                try
                {
                    {
                        await ImportMessages().ConfigureAwait(false);
                    }

                    _lastImports = null;
                }
                catch (Exception ex)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(MessageCollectorService), "StartCollecting", "Error while starting message collector service", null, ex);
                }
            }
        }
    }

    /// <summary>
    /// Client connected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnConnected()
    {
        try
        {
            await using (await _lockFactory.CreateLockAsync().ConfigureAwait(false))
            {
                await ImportMessages().ConfigureAwait(false);

                _lastImports = null;
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(MessageCollectorService), "StartCollecting", "Error while starting message collector service", null, ex);
        }
    }

    /// <summary>
    /// Client disconnected
    /// </summary>
    /// <param name="arg">Arguments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnDisconnected(Exception arg)
    {
        _importLimit = DateTime.Now.AddMinutes(-15);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Import messages
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ImportMessages()
    {
        var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

        await using (serviceProvider.ConfigureAwait(false))
        {
            var client = serviceProvider.GetService<DiscordSocketClient>();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var guildIds = dbFactory.GetRepository<GuildRepository>()
                                        .GetQuery()
                                        .Select(obj => obj.DiscordServerId)
                                        .ToList();

                foreach (var guildId in guildIds)
                {
                    var guild = client.GetGuild(guildId);

                    foreach (var category in guild.CategoryChannels)
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
                                LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(MessageCollectorService), "BulkInsert", dbFactory.LastError?.Message, dbFactory.LastError?.ToString());
                            }
                        }
                    }
                }
            }
        }
    }

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

        if (_importLimit < lastImport)
        {
            lastImport = _importLimit;
        }

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

    /// <summary>
    /// Message received
    /// </summary>
    /// <param name="e">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnMessageReceived(SocketMessage e)
    {
        if (e is IUserMessage)
        {
            if (e.Channel is IGuildChannel guildChannel)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var message = new DiscordMessageEntity
                                  {
                                      DiscordServerId = guildChannel.GuildId,
                                      DiscordAccountId = e.Author.Id,
                                      DiscordMessageId = e.Id,
                                      TimeStamp = e.CreatedAt.LocalDateTime
                                  };

                    if (e.Channel is SocketThreadChannel threadChannel)
                    {
                        message.DiscordThreadId = threadChannel.Id;
                        message.DiscordChannelId = threadChannel.ParentChannel.Id;
                    }
                    else
                    {
                        message.DiscordChannelId = e.Channel.Id;
                    }

                    dbFactory.GetRepository<DiscordMessageRepository>()
                             .Add(message);
                }
            }
        }

        return Task.CompletedTask;
    }

    #endregion // Methods

    #region SingletonLocatedServiceBase

    /// <inheritdoc />
    public override Task Initialize(IServiceProvider serviceProvider)
    {
#if RELEASE
        _client = serviceProvider.GetService<DiscordSocketClient>();

        Task.Run(StartCollecting);
#endif

        return base.Initialize(serviceProvider);
    }

#endregion // SingletonLocatedServiceBase

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        _lockFactory.Dispose();
    }

    #endregion // IDisposable
}