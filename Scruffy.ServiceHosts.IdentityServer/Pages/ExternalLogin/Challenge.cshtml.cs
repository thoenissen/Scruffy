using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.ExternalLogin;

/// <summary>
/// Challenge page
/// </summary>
[AllowAnonymous]
public class Challenge : PageModel
{
    #region Fields

    /// <summary>
    /// Fields
    /// </summary>
    private readonly IIdentityServerInteractionService _interactionService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="interactionService">Interaction service</param>
    public Challenge(IIdentityServerInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get method
    /// </summary>
    /// <param name="scheme">Scheme</param>
    /// <param name="returnUrl">Return url</param>
    /// <returns>Action result</returns>
    public IActionResult OnGet(string scheme, string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = "~/";
        }

        if (Url.IsLocalUrl(returnUrl) == false
         && _interactionService.IsValidReturnUrl(returnUrl) == false)
        {
            throw new InvalidOperationException("invalid return URL");
        }

        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Page("/ExternalLogin/callback"),
            Items =
            {
                { "returnUrl", returnUrl },
                { "scheme", scheme },
            }
        };

        return Challenge(props, scheme);
    }

    #endregion // Methods
}