using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

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
    private readonly DiscordClient _client;

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
    public MessageImportService(DiscordClient client)
    {
        _messages = new List<DiscordMessageBulkInsertData>();

        _client = client;
        _client.MessageCreated += OnMessageCreated;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Message created
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Argument</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        List<DiscordMessageBulkInsertData> importMessages = null;

        lock (_lock)
        {
            if (e.Guild != null
             && e.Author?.IsBot == false
             && e.Message.WebhookId == null)
            {
                _messages.Add(new DiscordMessageBulkInsertData
                              {
                                  ServerId = e.Guild.Id,
                                  ChannelId = e.Channel.Id,
                                  MessageId = e.Message.Id,
                                  TimeStamp = e.Message.CreationTimestamp.LocalDateTime,
                                  UserId = e.Author.Id
                              });

                if (_messages.Count > 100)
                {
                    importMessages = _messages;

                    _messages = new List<DiscordMessageBulkInsertData>();
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
        _client.MessageCreated -= OnMessageCreated;

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