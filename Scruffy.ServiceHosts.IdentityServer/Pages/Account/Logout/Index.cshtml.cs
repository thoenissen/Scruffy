using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Logout;

/// <summary>
/// Log out page
/// </summary>
[AllowAnonymous]
public class Index : PageModel
{
    #region Fields

    /// <summary>
    /// Interaction
    /// </summary>
    private readonly IIdentityServerInteractionService _interaction;

    /// <summary>
    /// Events
    /// </summary>
    private readonly IEventService _events;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="interaction">Interaction</param>
    /// <param name="events">Events</param>
    public Index(IIdentityServerInteractionService interaction, IEventService events)
    {
        _interaction = interaction;
        _events = events;
    }

    #endregion // Constructor

    /// <summary>
    /// Get method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IActionResult> OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            await _interaction.CreateLogoutContextAsync()
                              .ConfigureAwait(false);

            await HttpContext.SignOutAsync()
                             .ConfigureAwait(false);

            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()))
                         .ConfigureAwait(false);
        }

        return RedirectToPage("/Home/Index");
    }
}