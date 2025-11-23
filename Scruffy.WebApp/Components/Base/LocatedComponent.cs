using Microsoft.AspNetCore.Components;

using Scruffy.Services.Core.Localization;

namespace Scruffy.WebApp.Components.Base;

/// <summary>
/// Base class for components
/// </summary>
public class LocatedComponent : ComponentBase
{
    #region Properties

    /// <summary>
    /// Localization service
    /// </summary>
    [Inject]
    protected LocalizationService LocalizationService { get; private set; }

    /// <summary>
    /// Localization group
    /// </summary>
    protected LocalizationGroup LocalizationGroup => field ??= LocalizationService?.GetGroup(GetType().Name);

    #endregion // Properties
}