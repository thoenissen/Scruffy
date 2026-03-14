using Discord.WebSocket;

namespace Scruffy.Services.Discord;

/// <summary>
/// Container for temporary message components
/// </summary>
public abstract class TemporaryComponentsContainer : IAsyncDisposable
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="interactivityService">Interactivity service</param>
    internal TemporaryComponentsContainer(InteractivityService interactivityService)
    {
        InteractivityService = interactivityService;
        InteractivityService.AddComponentsContainer(this);
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Interactivity service
    /// </summary>
    protected InteractivityService InteractivityService { get; }

    #endregion // Properties

    #region Internal methods

    /// <summary>
    /// Check active buttons
    /// </summary>
    /// <param name="identification">Identification</param>
    /// <param name="component">Component</param>
    /// <returns>Is the component processed?</returns>
    internal abstract bool CheckButtonComponent(string identification, SocketMessageComponent component);

    /// <summary>
    /// Check active select menus
    /// </summary>
    /// <param name="identification">Identification</param>
    /// <param name="component">Component</param>
    /// <returns>Is the component processed?</returns>
    internal abstract bool CheckSelectMenuComponent(string identification, SocketMessageComponent component);

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

        InteractivityService.RemoveComponentsContainer(this);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Internal dispose Method
    /// </summary>
    protected abstract void Dispose();

    #endregion // IAsyncDisposable
}