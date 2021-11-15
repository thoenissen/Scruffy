using System.Threading;

namespace Scruffy.Services.Core;

/// <summary>
/// Factory to create locks to synchronize code blocks
/// </summary>
public sealed class LockFactory : IDisposable
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
    public LockFactory()
    {
        _semaphore = new SemaphoreSlim(1, 1);
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Create a lock
    /// </summary>
    /// <returns>The Container to hold the lock</returns>
    public async Task<IAsyncDisposable> CreateLockAsync()
    {
        await _semaphore.WaitAsync()
                        .ConfigureAwait(false);

        return new LockContainer(_semaphore);
    }

    /// <summary>
    /// Create a lock
    /// </summary>
    /// <returns>The Container to hold the lock</returns>
    public IDisposable CreateLock()
    {
        _semaphore.Wait();

        return new LockContainer(_semaphore);
    }

    #endregion // Methods

    #region Nested classes

    /// <summary>
    /// Container to release the lock
    /// </summary>
    private sealed class LockContainer : IAsyncDisposable, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="semaphore">Semaphore</param>
        public LockContainer(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _semaphore.Release();
        }

        #endregion

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            Dispose();

            return ValueTask.CompletedTask;
        }

        #endregion // AsyncDisposable
    }

    #endregion // Nested classes

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _semaphore.Dispose();
    }

    #endregion // IDisposable
}