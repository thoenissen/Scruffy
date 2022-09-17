using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Diagnostics;

/// <summary>
/// Diagnostics page
/// </summary>
[SecurityHeaders]
[Authorize]
public class Index : PageModel
{
    #region Properties

    /// <summary>
    /// View model
    /// </summary>
    public ViewModel View { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get route
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task<IActionResult> OnGet()
    {
        if (User.IsInRole("Developer"))
        {
            View = new ViewModel(await HttpContext.AuthenticateAsync().ConfigureAwait(false));

            return Page();
        }

        return Redirect("~/");
    }

    #endregion // Methods
}