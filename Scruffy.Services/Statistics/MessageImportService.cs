using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Services.Statistics;

namespace Scruffy.Services.Statistics;

/// <summary>
/// Message import
/// </summary>
public class MessageImportService : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordSocketClient _client;

    /// <summary>
    /// Lock
    /// </summary>
    private readonly object _lock = new ();

    /// <summary>
    /// Current messages
    /// </summary>
    private List<DiscordMessageBulkInsertData> _messages;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="client">Discord client</param>
    public MessageImportService(DiscordSocketClient client)
    {
        _messages = new List<DiscordMessageBulkInsertData>();

        _client = client;
        _client.MessageReceived += OnMessageReceived;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Message received
    /// </summary>
    /// <param name="e">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnMessageReceived(SocketMessage e)
    {
        List<DiscordMessageBulkInsertData> importMessages = null;

        if (e is IUserMessage)
        {
            lock (_lock)
            {
                if (e.Channel is IGuildChannel guildChannel)
                {
                    _messages.Add(new DiscordMessageBulkInsertData
                                  {
                                      ServerId = guildChannel.GuildId,
                                      ChannelId = e.Channel.Id,
                                      MessageId = e.Id,
                                      TimeStamp = e.CreatedAt.LocalDateTime,
                                      UserId = e.Author.Id
                                  });

                    if (_messages.Count > 100)
                    {
                        importMessages = _messages;

                        _messages = new List<DiscordMessageBulkInsertData>();
                    }
                }
            }
        }

        if (importMessages != null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                await dbFactory.GetRepository<DiscordMessageRepository>()
                               .BulkInsert(importMessages, false)
                               .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods

    #region IAsyncDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        _client.MessageReceived -= OnMessageReceived;

        List<DiscordMessageBulkInsertData> importMessages = null;

        lock (_lock)
        {
            importMessages = _messages;

            _messages = new List<DiscordMessageBulkInsertData>();
        }

        if (importMessages != null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                await dbFactory.GetRepository<DiscordMessageRepository>()
                               .BulkInsert(importMessages, false)
                               .ConfigureAwait(false);
            }
        }
    }

    #endregion // IAsyncDisposable
}