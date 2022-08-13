using System.Security.Claims;

using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;

using IdentityModel;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.ExternalLogin
{
    /// <summary>
    /// External log in callback page
    /// </summary>
    [AllowAnonymous]
    [SecurityHeaders]
    public class Callback : PageModel
    {
        #region Fields

        private readonly UserManager<UserEntity> _userManager;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<Callback> _logger;
        private readonly IEventService _events;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interaction">Identity server interaction</param>
        /// <param name="events">Events</param>
        /// <param name="logger">Logger</param>
        /// <param name="userManager">User manager</param>
        /// <param name="signInManager">Sign in manager</param>
        public Callback(IIdentityServerInteractionService interaction,
                        IEventService events,
                        ILogger<Callback> logger,
                        UserManager<UserEntity> userManager,
                        SignInManager<UserEntity> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _logger = logger;
            _events = events;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Get route
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task<IActionResult> OnGet()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme)
                                          .ConfigureAwait(false);
            if (result.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            var externalUser = result.Principal;

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            // lookup our user and external provider info
            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByLoginAsync(provider, providerUserId)
                                         .ConfigureAwait(false);

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            CaptureExternalLoginContext(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            await _signInManager.SignInWithClaimsAsync(user, localSignInProps, additionalLocalClaims)
                                .ConfigureAwait(false);

            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme)
                             .ConfigureAwait(false);

            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl)
                                            .ConfigureAwait(false);

            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id.ToString(), user.UserName, true, context?.Client.ClientId))
                         .ConfigureAwait(false);

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    return this.LoadingPage(returnUrl);
                }
            }

            return Redirect(returnUrl);
        }

        /// <summary>
        /// Capture external claims
        /// </summary>
        /// <param name="externalResult">External result</param>
        /// <param name="localClaims">Local claims</param>
        /// <param name="localSignInProps">Local sign in props</param>
        private void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            if (externalResult.Properties?.Items.TryGetValue("scheme", out var scheme) == true)
            {
                localClaims.Add(new Claim(JwtClaimTypes.IdentityProvider, scheme));
            }

            var sid = externalResult.Principal?.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            var idToken = externalResult.Properties?.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[]
                                             {
                                                 new AuthenticationToken
                                                 {
                                                     Name = "id_token",
                                                     Value = idToken
                                                 }
                                             });
            }
        }

        #endregion // Methods
    }
}