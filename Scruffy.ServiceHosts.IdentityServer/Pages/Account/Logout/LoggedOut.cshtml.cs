using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Logout;

/// <summary>
/// Logged out page
/// </summary>
[SecurityHeaders]
[AllowAnonymous]
public class LoggedOut : PageModel
{
    #region Fields

    private readonly IIdentityServerInteractionService _interactionService;

    #endregion // Fields

    /// <summary>
    /// Logged out
    /// </summary>
    /// <param name="interactionService">Interaction service</param>
    public LoggedOut(IIdentityServerInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    #region Properties

    /// <summary>
    /// View
    /// </summary>
    public LoggedOutViewModel View { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get method
    /// </summary>
    /// <param name="logoutId">Log out id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task OnGet(string logoutId)
    {
        var logout = await _interactionService.GetLogoutContextAsync(logoutId)
                                              .ConfigureAwait(false);

        View = new LoggedOutViewModel
               {
                   PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                   ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout.ClientName,
                   SignOutIframeUrl = logout?.SignOutIFrameUrl
               };
    }

    #endregion // Methods
}