using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages;

/// <summary>
/// Lading page
/// </summary>
[AllowAnonymous]
public class Index : PageModel
{
    #region Methods

    /// <summary>
    /// Get method
    /// </summary>
    /// <returns>Action result</returns>
    public IActionResult OnGet() => RedirectToPage("/Home/Index");

    #endregion // Methods
}