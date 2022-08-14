using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;

using IdentityModel;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Logout
{
    /// <summary>
    /// Log out page
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        #region Fields

        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="signInManager">Sign in manager</param>
        /// <param name="interaction">Interaction</param>
        /// <param name="events">Events</param>
        public Index(SignInManager<UserEntity> signInManager, IIdentityServerInteractionService interaction, IEventService events)
        {
            _signInManager = signInManager;
            _interaction = interaction;
            _events = events;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Logout id
        /// </summary>
        [BindProperty]
        public string LogoutId { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Get route
        /// </summary>
        /// <param name="logoutId">Log out id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task<IActionResult> OnGet(string logoutId)
        {
            LogoutId = logoutId;

            var showLogoutPrompt = true;

            if (User.Identity?.IsAuthenticated != true)
            {
                showLogoutPrompt = false;
            }
            else
            {
                var context = await _interaction.GetLogoutContextAsync(LogoutId)
                                                .ConfigureAwait(false);

                if (context?.ShowSignoutPrompt == false)
                {
                    showLogoutPrompt = false;
                }
            }

            return showLogoutPrompt == false
                       ? await OnPost()
                             .ConfigureAwait(false)
                       : Page();
        }

        /// <summary>
        /// Post route
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task<IActionResult> OnPost()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                LogoutId ??= await _interaction.CreateLogoutContextAsync()
                                               .ConfigureAwait(false);

                await _signInManager.SignOutAsync()
                                    .ConfigureAwait(false);

                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()))
                             .ConfigureAwait(false);

                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

                if (idp is not null
                       and not IdentityServerConstants.LocalIdentityProvider)
                {
                    if (await HttpContext.GetSchemeSupportsSignOutAsync(idp)
                                         .ConfigureAwait(false))
                    {
                        var url = Url.Page("/Account/Logout/Loggedout", new { logoutId = LogoutId });

                        return SignOut(new AuthenticationProperties { RedirectUri = url }, idp);
                    }
                }
            }

            return RedirectToPage("/Account/Logout/LoggedOut", new { logoutId = LogoutId });
        }

        #endregion // Methods
    }
}