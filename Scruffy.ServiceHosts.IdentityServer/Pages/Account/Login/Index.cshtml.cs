using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Login
{
    /// <summary>
    /// Login page
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        #region Fields

        private readonly UserManager<UserEntity> _userManager;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IIdentityProviderStore _identityProviderStore;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interaction">Interaction with the identity server</param>
        /// <param name="clientStore">Client store</param>
        /// <param name="schemeProvider">Scheme provider</param>
        /// <param name="identityProviderStore">Identity provider store</param>
        /// <param name="events">Events</param>
        /// <param name="userManager">User manager</param>
        /// <param name="signInManager">Sign-In manager</param>
        public Index(IIdentityServerInteractionService interaction,
                     IClientStore clientStore,
                     IAuthenticationSchemeProvider schemeProvider,
                     IIdentityProviderStore identityProviderStore,
                     IEventService events,
                     UserManager<UserEntity> userManager,
                     SignInManager<UserEntity> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _identityProviderStore = identityProviderStore;
            _events = events;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// View data
        /// </summary>
        public ViewModel View { get; set; }

        /// <summary>
        /// Input data
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Get route
        /// </summary>
        /// <param name="returnUrl">Return url</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task<IActionResult> OnGet(string returnUrl)
        {
            await BuildModelAsync(returnUrl).ConfigureAwait(false);

            return Page();
        }

        /// <summary>
        /// Post route
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task<IActionResult> OnPost()
        {
            var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl)
                                            .ConfigureAwait(false);

            if (Input.Button != "login")
            {
                if (context != null)
                {
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied)
                                      .ConfigureAwait(false);

                    return context.IsNativeClient()
                               ? this.LoadingPage(Input.ReturnUrl)
                               : Redirect(Input.ReturnUrl);
                }

                return Redirect("~/");
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberLogin, lockoutOnFailure: true)
                                                 .ConfigureAwait(false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(Input.UserName)
                                                 .ConfigureAwait(false);

                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId))
                                 .ConfigureAwait(false);

                    return context != null
                               ? context.IsNativeClient()
                                     ? this.LoadingPage(Input.ReturnUrl)
                                     : Redirect(Input.ReturnUrl)
                               : Url.IsLocalUrl(Input.ReturnUrl)
                                   ? Redirect(Input.ReturnUrl)
                                   : string.IsNullOrEmpty(Input.ReturnUrl)
                                       ? Redirect("~/")
                                       : throw new Exception("invalid return URL");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(Input.UserName, "invalid credentials", clientId: context?.Client.ClientId))
                             .ConfigureAwait(false);

                ModelState.AddModelError(string.Empty, "Invalid user name or password");
            }

            await BuildModelAsync(Input.ReturnUrl).ConfigureAwait(false);

            return Page();
        }

        /// <summary>
        /// Building the page
        /// </summary>
        /// <param name="returnUrl">Return url</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        private async Task BuildModelAsync(string returnUrl)
        {
            Input = new InputModel
                    {
                        ReturnUrl = returnUrl
                    };

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl)
                                            .ConfigureAwait(false);

            if (context?.IdP != null
             && await _schemeProvider.GetSchemeAsync(context.IdP)
                                     .ConfigureAwait(false) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                View = new ViewModel();

                Input.UserName = context.LoginHint;

                if (!local)
                {
                    View.ExternalProviders = new[] { new ViewModel.ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync()
                                               .ConfigureAwait(false);

            var providers = schemes.Where(x => x.DisplayName != null)
                                   .Select(x => new ViewModel.ExternalProvider
                                                {
                                                    DisplayName = x.DisplayName ?? x.Name,
                                                    AuthenticationScheme = x.Name
                                                })
                                   .ToList();

            var dyanmicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync()
                                                              .ConfigureAwait(false))
                                 .Where(x => x.Enabled)
                                 .Select(x => new ViewModel.ExternalProvider
                                              {
                                                  AuthenticationScheme = x.Scheme,
                                                  DisplayName = x.DisplayName
                                              });

            providers.AddRange(dyanmicSchemes);

            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId)
                                               .ConfigureAwait(false);

                if (client != null)
                {
                    if (client.IdentityProviderRestrictions != null
                     && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            View = new ViewModel
                   {
                       ExternalProviders = providers.ToArray()
                   };
        }

        #endregion // Methods
    }
}