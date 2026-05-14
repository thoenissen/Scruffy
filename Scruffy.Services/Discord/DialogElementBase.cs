using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element
/// </summary>
public abstract class DialogElementBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogElementBase(LocalizationService localizationService)
    {
        LocalizationGroup = localizationService.GetGroup(GetType().Name);
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Localization group
    /// </summary>
    public LocalizationGroup LocalizationGroup { get; }

    /// <summary>
    /// Command context
    /// </summary>
    public IContextContainer CommandContext { get; private set; }

    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; private set; }

    /// <summary>
    /// Current dialog context
    /// </summary>
    public DialogContext DialogContext { get; private set; }

    /// <summary>
    /// Is the element ephermeral?
    /// </summary>
    public bool IsEphermeral { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Initializing
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="dialogContext">Dialog context</param>
    internal void Initialize(IContextContainer commandContext, IServiceProvider serviceProvider, DialogContext dialogContext)
    {
        CommandContext = commandContext;
        ServiceProvider = serviceProvider;
        DialogContext = dialogContext;
    }

    /// <summary>
    /// Execution of the element
    /// </summary>
    /// <returns>Result</returns>
    internal abstract Task<object> InternalRun();

    #endregion // Methods
}