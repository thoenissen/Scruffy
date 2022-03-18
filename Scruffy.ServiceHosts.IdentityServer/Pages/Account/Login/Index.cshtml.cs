using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Login;

/// <summary>
/// Log in page
/// </summary>
[AllowAnonymous]
public class Index : PageModel
{
    #region Methods

    /// <summary>
    /// Get method
    /// </summary>
    /// <param name="returnUrl">Return url</param>
    /// <returns>Action result</returns>
    public IActionResult OnGet(string returnUrl) => RedirectToPage("/ExternalLogin/Challenge", new { scheme = "Discord", returnUrl });

    #endregion // Methods
}