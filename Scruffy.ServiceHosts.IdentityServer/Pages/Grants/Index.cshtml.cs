using System.ComponentModel.DataAnnotations;

using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Grants
{
    /// <summary>
    /// Constructor
    /// </summary>
    [SecurityHeaders]
    [Authorize]
    public class Index : PageModel
    {
        #region Fields

        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clients;
        private readonly IResourceStore _resources;
        private readonly IEventService _events;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interaction">Identity server interaction</param>
        /// <param name="clients">Clients</param>
        /// <param name="resources">Resources</param>
        /// <param name="events">Events</param>
        public Index(IIdentityServerInteractionService interaction,
                     IClientStore clients,
                     IResourceStore resources,
                     IEventService events)
        {
            _interaction = interaction;
            _clients = clients;
            _resources = resources;
            _events = events;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// View model
        /// </summary>
        public ViewModel View { get; set; }

        /// <summary>
        /// Client id
        /// </summary>
        [BindProperty]
        [Required]
        public string ClientId { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Get route
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task OnGet()
        {
            var grants = await _interaction.GetAllUserGrantsAsync()
                                           .ConfigureAwait(false);

            var list = new List<GrantViewModel>();
            foreach (var grant in grants)
            {
                var client = await _clients.FindClientByIdAsync(grant.ClientId)
                                           .ConfigureAwait(false);
                if (client != null)
                {
                    var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes)
                                                    .ConfigureAwait(false);

                    var item = new GrantViewModel
                               {
                                   ClientId = client.ClientId,
                                   ClientName = client.ClientName ?? client.ClientId,
                                   ClientLogoUrl = client.LogoUri,
                                   ClientUrl = client.ClientUri,
                                   Description = grant.Description,
                                   Created = grant.CreationTime,
                                   Expires = grant.Expiration,
                                   IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                                   ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
                               };

                    list.Add(item);
                }
            }

            View = new ViewModel
                   {
                       Grants = list
                   };
        }

        /// <summary>
        /// Post route
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task<IActionResult> OnPost()
        {
            await _interaction.RevokeUserConsentAsync(ClientId)
                              .ConfigureAwait(false);

            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), ClientId))
                         .ConfigureAwait(false);

            return RedirectToPage("/Grants/Index");
        }

        #endregion // Methods
    }
}