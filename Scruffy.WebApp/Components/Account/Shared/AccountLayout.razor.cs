using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Scruffy.WebApp.Components.Account.Shared;

/// <summary>
/// Account layout
/// </summary>
public partial class AccountLayout
{
    #region Properties

    /// <summary>
    /// Http context
    /// </summary>
    [CascadingParameter]
    private HttpContext HttpContext { get; set; }

    /// <summary>
    /// Navigation manager
    /// </summary>
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    #endregion // Properties

    #region ComponentBase

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (HttpContext is null)
        {
            // If this code runs, we're currently rendering in interactive mode, so there is no HttpContext.
            // The identity pages need to set cookies, so they require an HttpContext. To achieve this we
            // must transition back from interactive mode to a server-rendered page.
            NavigationManager.Refresh(forceReload: true);
        }
    }

    #endregion // ComponentBase
}