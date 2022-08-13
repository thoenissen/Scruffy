using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages
{
    /// <summary>
    /// Main page
    /// </summary>
    [AllowAnonymous]
    public class Index : PageModel
    {
        #region Properties

        /// <summary>
        /// Is developer?
        /// </summary>
        public bool IsDeveloper => User?.IsInRole("Developer") == true;

        #endregion // Properties
    }
}