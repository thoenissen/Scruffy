using System.Threading;

using Scruffy.Services.Core.Exceptions;

namespace Scruffy.Services.Discord;

/// <summary>
/// Waiting for message data
/// </summary>
/// <typeparam name="T">Type of waiting for object</typeparam>
internal sealed class InteractionWaitEntry<T> : IDisposable
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
    private CancellationTokenSource _cancellationTokenSource;

    #endregion // Fields

    #region Constructor

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

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Command cancellation
    /// </summary>
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    #endregion // Properties

    #region Methods

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
        }
    }

    #endregion // Methods

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    #endregion // IDisposable
}