using Microsoft.AspNetCore.Components;

using Scruffy.Services.Core.Localization;

namespace Scruffy.WebApp.Components.Base;

/// <summary>
/// Base class for components
/// </summary>
public class LocatedComponent : ComponentBase
{
    #region Fields

    /// <summary>
    /// Localization group
    /// </summary>
    private LocalizationGroup _localizationGroup;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Localization service
    /// </summary>
    [Inject]
    protected LocalizationService LocalizationService { get; private set; }

    /// <summary>
    /// Localization group
    /// </summary>
    protected LocalizationGroup LocalizationGroup => _localizationGroup ??= LocalizationService?.GetGroup(GetType().Name);

    #endregion // Properties
}