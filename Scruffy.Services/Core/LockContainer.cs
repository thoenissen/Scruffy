using System.Threading;

namespace Scruffy.Services.Core;

/// <summary>
/// Container to release the lock
/// </summary>
internal sealed class LockContainer : IAsyncDisposable, IDisposable
{
    #region Fields

    /// <summary>
    /// Semaphore
    /// </summary>
    private readonly SemaphoreSlim _semaphore;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="semaphore">Semaphore</param>
    internal LockContainer(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }

    #endregion // Constructor

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
    /// </summary>
    public void Dispose()
    {
        _semaphore.Release();
    }

    #endregion // IDisposable

    #region IAsyncDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation</returns>
    public ValueTask DisposeAsync()
    {
        Dispose();

        return ValueTask.CompletedTask;
    }

    #endregion // IAsyncDisposable
}