using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Redirect;

/// <summary>
/// Redirect page
/// </summary>
[AllowAnonymous]
public class IndexModel : PageModel
{
    #region Properties

    /// <summary>
    /// Redirect uri
    /// </summary>
    public string RedirectUri { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get method
    /// </summary>
    /// <param name="redirectUri">Redirect uri</param>
    /// <returns>Action result</returns>
    public IActionResult OnGet(string redirectUri)
    {
        if (!Url.IsLocalUrl(redirectUri))
        {
            return RedirectToPage("/Error/Index");
        }

        RedirectUri = redirectUri;

        return Page();
    }

    #endregion // Methods
}