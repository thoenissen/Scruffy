using System;

using Microsoft.AspNetCore.Components;

namespace Scruffy.WebApp.Components.Account.Shared;

/// <summary>
/// Redirecting to log in
/// </summary>
public class RedirectToLogin : ComponentBase
{
    #region Properties

    /// <summary>
    /// Navigation manger
    /// </summary>
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    #endregion // Properties

    #region ComponentBase

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo($"Account/Login?returnUrl={Uri.EscapeDataString(NavigationManager.Uri)}", forceLoad: true);
    }

    #endregion // ComponentBase
}