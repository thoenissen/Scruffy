using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element which response with an interactive component
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class InteractionDialogElementBase<TData> : DialogElementBase<TData>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected InteractionDialogElementBase(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor
}