using System.Threading;

using Discord.WebSocket;

using Scruffy.Services.Core.Exceptions;

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

/// <summary>
/// Container for temporary message components
/// </summary>
/// <typeparam name="TIdentification">Typ of the identification</typeparam>
public class TemporaryComponentsContainer<TIdentification> : TemporaryComponentsContainer
{
    #region Fields

    /// <summary>
    /// Check function
    /// </summary>
    private readonly Func<SocketMessageComponent, bool> _checkFunction;

    /// <summary>
    /// Cancellation token source
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Task source
    /// </summary>
    private TaskCompletionSource<(SocketMessageComponent Component, TIdentification Identification)> _taskSource;

    /// <summary>
    /// Buttons
    /// </summary>
    private Dictionary<string, TIdentification> _buttons;

    /// <summary>
    /// Select menus
    /// </summary>
    private Dictionary<string, TIdentification> _selectMenus;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="interactivityService">Interactivity service</param>
    /// <param name="checkFunction">Check interaction</param>
    public TemporaryComponentsContainer(InteractivityService interactivityService, Func<SocketMessageComponent, bool> checkFunction)
        : base(interactivityService)
    {
        _checkFunction = checkFunction;
        _cancellationTokenSource = new CancellationTokenSource();
        _taskSource = new TaskCompletionSource<(SocketMessageComponent, TIdentification)>();
        _buttons = new Dictionary<string, TIdentification>();
        _selectMenus = new Dictionary<string, TIdentification>();
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
    public string AddButton(TIdentification identification)
    {
        var customId = Guid.NewGuid().ToString("N");

        _buttons[customId] = identification;

        return "temporary;button;" + customId;
    }

    /// <summary>
    /// Adding a new select menu
    /// </summary>
    /// <param name="identification">Identification</param>
    /// <returns>Custom-Id</returns>
    public string AddSelectMenu(TIdentification identification)
    {
        var customId = Guid.NewGuid().ToString("N");

        _selectMenus[customId] = identification;

        return "temporary;selectMenu;" + customId;
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

    /// <inheritdoc/>
    internal override bool CheckButtonComponent(string identification, SocketMessageComponent component)
    {
        if (_buttons.TryGetValue(identification, out var customerIdentification))
        {
            if (_checkFunction(component))
            {
                _taskSource.TrySetResult((component, customerIdentification));
            }
            else
            {
                component.RespondAsync(InteractivityService.LocalizationGroup.GetText("DialogNotAssigned", "You can only interact with dialogs that are assigned to you."), ephemeral: true);
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    internal override bool CheckSelectMenuComponent(string identification, SocketMessageComponent component)
    {
        if (_selectMenus.TryGetValue(identification, out var customerIdentification))
        {
            if (_checkFunction(component))
            {
                _taskSource.TrySetResult((component, customerIdentification));
            }
            else
            {
                component.RespondAsync(InteractivityService.LocalizationGroup.GetText("DialogNotAssigned", "You can only interact with dialogs that are assigned to you."), ephemeral: true);
            }

            return true;
        }

        return false;
    }

    #endregion // TemporaryComponentsContainer

    #region IDisposable

    /// <inheritdoc/>
    protected override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    #endregion // IDisposable
}