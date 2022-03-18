using Duende.IdentityServer;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Token
{
    /// <summary>
    /// Token creation
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        #region Fields

        /// <summary>
        /// Identity server tools
        /// </summary>
        private readonly IdentityServerTools _identityServerTools;

        /// <summary>
        /// Environment
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="identityServerTools">Identity server tools</param>
        /// <param name="environment">Environment</param>
        public IndexModel(IdentityServerTools identityServerTools, IWebHostEnvironment environment)
        {
            _identityServerTools = identityServerTools;
            _environment = environment;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Get method
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task<IActionResult> OnGet()
        {
            if (_environment.IsDevelopment())
            {
                var issuer = "http://" + Request.Host.Value;

                var token = await _identityServerTools.IssueJwtAsync(30000, issuer, User.Claims)
                                                      .ConfigureAwait(false);

                return new JsonResult(new { Token = token });
            }

            return RedirectToPage("/Account/AccessDenied");
        }

        #endregion // Methods
    }
}
