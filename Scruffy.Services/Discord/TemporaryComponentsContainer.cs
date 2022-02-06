using System.Threading;

using Discord.WebSocket;

using Scruffy.Services.Core.Exceptions;

namespace Scruffy.Services.Discord;

/// <summary>
/// Container for temporary message components
/// </summary>
public abstract class TemporaryComponentsContainer : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Interaction service
    /// </summary>
    private readonly InteractionService _interactionService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="interactionService">Interaction service</param>
    internal TemporaryComponentsContainer(InteractionService interactionService)
    {
        _interactionService = interactionService;
        _interactionService.AddComponentsContainer(this);
    }

    #endregion // Constructor

    #region Internal methods

    /// <summary>
    /// Check active buttons
    /// </summary>
    /// <param name="component">Component</param>
    internal abstract void CheckComponent(SocketMessageComponent component);

    #endregion // Internal methods

    #region IAsyncDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        Dispose();

        _interactionService.RemoveComponentsContainer(this);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Internal dispose Method
    /// </summary>
    protected abstract void Dispose();

    #endregion // IAsyncDisposable
}

/// <summary>
/// Container for temporary message components
/// </summary>
/// <typeparam name="TIdentification">Typ of the identification</typeparam>
public class TemporaryComponentsContainer<TIdentification> : TemporaryComponentsContainer
{
    #region Fields

    /// <summary>
    /// Cancellation token source
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Task source
    /// </summary>
    private TaskCompletionSource<(SocketMessageComponent Component, TIdentification Identification)> _taskSource;

    /// <summary>
    /// Components
    /// </summary>
    private Dictionary<string, TIdentification> _components;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="interactionService">Interaction service</param>
    public TemporaryComponentsContainer(InteractionService interactionService)
        : base(interactionService)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _taskSource = new TaskCompletionSource<(SocketMessageComponent, TIdentification)>();
        _components = new Dictionary<string, TIdentification>();
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Task
    /// </summary>
    public Task<(SocketMessageComponent Component, TIdentification Identification)> Task => _taskSource.Task;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Adding a new button
    /// </summary>
    /// <param name="identification">Identification</param>
    /// <returns>Custom-Id</returns>
    public string AddComponent(TIdentification identification)
    {
        var customId = "T_" + Guid.NewGuid().ToString("N");

        _components[customId] = identification;

        return customId;
    }

    /// <summary>
    /// Starting timeout
    /// </summary>
    public void StartTimeout()
    {
        System.Threading.Tasks.Task.Delay(60_000, _cancellationTokenSource.Token)
              .ContinueWith(obj => _taskSource.TrySetException(new ScruffyTimeoutException()), _cancellationTokenSource.Token);
    }

    #endregion // Methods

    #region TemporaryComponentsContainer

    /// <summary>
    /// Check active buttons
    /// </summary>
    /// <param name="component">Component</param>
    internal override void CheckComponent(SocketMessageComponent component)
    {
        if (_components.TryGetValue(component.Data.CustomId, out var identification))
        {
            _taskSource.TrySetResult((component, identification));
        }
    }

    #endregion // TemporaryComponentsContainer

    #region IDisposable

    /// <summary>
    /// Internal dispose method
    /// </summary>
    protected override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    #endregion // IDisposable
}