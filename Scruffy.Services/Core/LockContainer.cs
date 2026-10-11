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

    /// <inheritdoc/>
    public void Dispose()
    {
        _semaphore.Release();
    }

    #endregion // IDisposable

    #region IAsyncDisposable

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Dispose();

        return ValueTask.CompletedTask;
    }

    #endregion // IAsyncDisposable
}