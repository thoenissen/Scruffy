using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Redirect
{
    /// <summary>
    /// Redirect page
    /// </summary>
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        #region Properties

        /// <summary>
        /// Redirect url
        /// </summary>
        public string RedirectUri { get; set; }

        #endregion // Properties

        #region Properties

        /// <summary>
        /// Get route
        /// </summary>
        /// <param name="redirectUri">Redirect url</param>
        /// <returns>Action result</returns>
        public IActionResult OnGet(string redirectUri)
        {
            if (Url.IsLocalUrl(redirectUri) == false)
            {
                return RedirectToPage("/Error/Index");
            }

            RedirectUri = redirectUri;

            return Page();
        }

        #endregion // Properties
    }
}