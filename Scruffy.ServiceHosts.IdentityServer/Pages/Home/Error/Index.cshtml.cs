using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Home.Error;

/// <summary>
/// Error page
/// </summary>
[AllowAnonymous]
public class Index : PageModel
{
    #region Fields

    /// <summary>
    /// Interaction service
    /// </summary>
    private readonly IIdentityServerInteractionService _interaction;

    /// <summary>
    /// Environment
    /// </summary>
    private readonly IWebHostEnvironment _environment;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="interaction">Interaction</param>
    /// <param name="environment">Environment</param>
    public Index(IIdentityServerInteractionService interaction, IWebHostEnvironment environment)
    {
        _interaction = interaction;
        _environment = environment;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// View
    /// </summary>
    public ViewModel View { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get method
    /// </summary>
    /// <param name="errorId">Error id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task OnGet(string errorId)
    {
        View = new ViewModel();

        var message = await _interaction.GetErrorContextAsync(errorId)
                                        .ConfigureAwait(false);
        if (message != null)
        {
            View.Error = message;

            if (_environment.IsDevelopment() == false)
            {
                message.ErrorDescription = null;
            }
        }
    }

    #endregion // Methods
}