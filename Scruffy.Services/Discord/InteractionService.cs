using System.Collections.Concurrent;
using System.Threading;

using Discord;
using Discord.WebSocket;

using Scruffy.Services.Core.Exceptions;

namespace Scruffy.Services.Discord
{
    /// <summary>
    /// Waiting for message data
    /// </summary>
    /// <typeparam name="T">Type of waiting for object</typeparam>
    internal class InteractionWaitEntry<T>
    {
        #region Fields

        /// <summary>
        /// Completion task
        /// </summary>
        private readonly TaskCompletionSource<T> _taskCompletionSource;

        /// <summary>
        /// Check function
        /// </summary>
        private readonly Func<T, bool> _checkMessageFunction;

        /// <summary>
        /// Command cancellation
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource;

        #endregion // Fields

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taskCompletionSource">Task source</param>
        /// <param name="checkMessageFunction">Check message function</param>
        public InteractionWaitEntry(TaskCompletionSource<T> taskCompletionSource, Func<T, bool> checkMessageFunction)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _taskCompletionSource = taskCompletionSource;
            _checkMessageFunction = checkMessageFunction;
        }

        /// <summary>
        /// Command cancellation
        /// </summary>
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// Check message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Is the check successfully?</returns>
        public bool CheckMessage(T message)
        {
            var success = false;

            if (_checkMessageFunction(message))
            {
                if (_taskCompletionSource.TrySetResult(message))
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();

                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// Set command timeout
        /// </summary>
        public void SetTimeOut()
        {
            if (_taskCompletionSource.TrySetException(new ScruffyTimeoutException()))
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }
    }

    /// <summary>
    /// User interaction service
    /// </summary>
    public sealed class InteractionService : IDisposable
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private DiscordSocketClient _client;

        /// <summary>
        /// Token source
        /// </summary>
        private CancellationTokenSource _tokenSource;

        /// <summary>
        /// A new message was created
        /// </summary>
        private AutoResetEvent _messageEvent;

        /// <summary>
        /// Created messages
        /// </summary>
        private ConcurrentQueue<IUserMessage> _messages;

        /// <summary>
        /// Wait for messages
        /// </summary>
        private List<InteractionWaitEntry<IUserMessage>> _waitForMessage;

        /// <summary>
        /// A new reaction was created
        /// </summary>
        private AutoResetEvent _reactionEvent;

        /// <summary>
        /// Created reactions
        /// </summary>
        private ConcurrentQueue<IReaction> _reactions;

        /// <summary>
        /// Wait for reactions
        /// </summary>
        private List<InteractionWaitEntry<IReaction>> _waitForReaction;

        /// <summary>
        /// IDisposable
        /// </summary>
        private bool _disposed;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Discord client</param>
        public InteractionService(DiscordSocketClient client)
        {
            _tokenSource = new CancellationTokenSource();

            _waitForMessage = new List<InteractionWaitEntry<IUserMessage>>();
            _messageEvent = new AutoResetEvent(false);
            _messages = new ConcurrentQueue<IUserMessage>();

            _waitForReaction = new List<InteractionWaitEntry<IReaction>>();
            _reactionEvent = new AutoResetEvent(false);
            _reactions = new ConcurrentQueue<IReaction>();

            Task.Run(() => CheckMessages(_tokenSource.Token), _tokenSource.Token);
            Task.Run(() => CheckReactions(_tokenSource.Token), _tokenSource.Token);

            _client = client;
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;
        }

        #endregion // Constructor

        #region Public methods

        /// <summary>
        /// Wait for a specific message
        /// </summary>
        /// <param name="checkMessage">Function to check the message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<IUserMessage> WaitForMessageAsync(Func<IUserMessage, bool> checkMessage)
        {
            var taskSource = new TaskCompletionSource<IUserMessage>();

            var waitEntry = new InteractionWaitEntry<IUserMessage>(taskSource, checkMessage);

            lock (_waitForMessage)
            {
                _waitForMessage.Add(waitEntry);
            }

            Task.Delay(15_000, waitEntry.CancellationToken)
                .ContinueWith(t =>
                              {
                                  waitEntry.SetTimeOut();

                                  lock (_waitForMessage)
                                  {
                                      _waitForMessage.Remove(waitEntry);
                                  }
                              });

            return taskSource.Task;
        }

        /// <summary>
        /// Waiting for a reaction
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="user">User</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<IReaction> WaitForReactionAsync(IUserMessage message, IUser user)
        {
            var taskSource = new TaskCompletionSource<IReaction>();

            var waitEntry = new InteractionWaitEntry<IReaction>(taskSource,
                                                                obj => obj is SocketReaction socketReaction
                                                                    && socketReaction.MessageId == message.Id
                                                                    && socketReaction.UserId == user.Id);

            lock (_waitForReaction)
            {
                _waitForReaction.Add(waitEntry);
            }

            Task.Delay(15_000, waitEntry.CancellationToken)
                .ContinueWith(t =>
                              {
                                  waitEntry.SetTimeOut();

                                  lock (_waitForReaction)
                                  {
                                      _waitForReaction.Remove(waitEntry);
                                  }
                              });

            return taskSource.Task;
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// A new message was created
        /// </summary>
        /// <param name="arg">Arguments</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private Task OnMessageReceived(SocketMessage arg)
        {
            if (arg is IUserMessage message)
            {
                _messages.Enqueue(message);
                _messageEvent.Set();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// A new reaction was added
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="message">Message</param>
        /// <param name="reaction">Reaction</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> user, Cacheable<IMessageChannel, ulong> message, SocketReaction reaction)
        {
            _reactions.Enqueue(reaction);
            _reactionEvent.Set();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Check incoming messages
        /// </summary>
        /// <param name="tokenSourceToken">Token source</param>
        private void CheckMessages(CancellationToken tokenSourceToken)
        {
            while (tokenSourceToken.IsCancellationRequested == false)
            {
                while (_messages.TryDequeue(out var message))
                {
                    List<InteractionWaitEntry<IUserMessage>> currentChecks;

                    lock (_waitForMessage)
                    {
                        currentChecks = _waitForMessage.ToList();
                    }

                    foreach (var waitEntry in currentChecks.Where(waitEntry => waitEntry.CheckMessage(message)))
                    {
                        lock (_waitForMessage)
                        {
                            _waitForMessage.Remove(waitEntry);
                        }
                    }
                }

                WaitHandle.WaitAny(new[] { _messageEvent, tokenSourceToken.WaitHandle });
            }
        }

        /// <summary>
        /// Check incoming reactions
        /// </summary>
        /// <param name="tokenSourceToken">Token source</param>
        private void CheckReactions(CancellationToken tokenSourceToken)
        {
            while (tokenSourceToken.IsCancellationRequested == false)
            {
                while (_reactions.TryDequeue(out var reaction))
                {
                    List<InteractionWaitEntry<IReaction>> currentChecks;

                    lock (_waitForReaction)
                    {
                        currentChecks = _waitForReaction.ToList();
                    }

                    foreach (var waitEntry in currentChecks.Where(waitEntry => waitEntry.CheckMessage(reaction)))
                    {
                        lock (_waitForReaction)
                        {
                            _waitForReaction.Remove(waitEntry);
                        }
                    }
                }

                WaitHandle.WaitAny(new[] { _reactionEvent, tokenSourceToken.WaitHandle });
            }
        }

        #endregion // Private methods

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed == false)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();

                _messageEvent.Dispose();
                _reactionEvent.Dispose();

                _client.MessageReceived += OnMessageReceived;
                _client.ReactionAdded += OnReactionAdded;
                _client = null;

                _disposed = true;
            }
        }

        #endregion // IDisposable
    }
}
