using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.WebApp.Components.Account.Pages;

/// <summary>
/// Login through an external provider
/// </summary>
[Route("/Account/ExternalLogin")]
public class ExternalLogin : ComponentBase
{
    #region Constants

    /// <summary>
    /// Log in callback
    /// </summary>
    public const string LoginCallbackAction = "LoginCallback";

    #endregion // Constants

    #region Properties

    #region Injection

    /// <summary>
    /// Sign in manger
    /// </summary>
    [Inject]
    private SignInManager<UserEntity> SignInManager { get; set; }

    /// <summary>
    /// Redirect manager
    /// </summary>
    [Inject]
    private IdentityRedirectManager RedirectManager { get; set; }

    /// <summary>
    /// Logger
    /// </summary>
    [Inject]
    private ILogger<ExternalLogin> Logger { get; set; }

    #endregion // Injection

    #region Parameters

    /// <summary>
    /// Current context
    /// </summary>
    [CascadingParameter]
    private HttpContext HttpContext { get; set; }

    /// <summary>
    /// Remote error
    /// </summary>
    [SupplyParameterFromQuery]
    private string RemoteError { get; set; }

    /// <summary>
    /// Return url
    /// </summary>
    [SupplyParameterFromQuery]
    private string ReturnUrl { get; set; }

    /// <summary>
    /// Action
    /// </summary>
    [SupplyParameterFromQuery]
    public string Action { get; set; }

    #endregion // Parameters

    #endregion // Properties

    #region ComponentBase

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        if (HttpMethods.IsGet(HttpContext.Request.Method) == false)
        {
            return;
        }

        if (RemoteError is not null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", $"Error from external provider: {RemoteError}", HttpContext);

            return;
        }

        var externalLoginInfo = await SignInManager.GetExternalLoginInfoAsync()
                                                   .ConfigureAwait(false);

        if (externalLoginInfo is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", "Error loading external login information.", HttpContext);

            return;
        }

        if (Action != LoginCallbackAction)
        {
            RedirectManager.RedirectTo(string.Empty);

            return;
        }

        var result = await SignInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, false, true)
                                        .ConfigureAwait(false);

        if (result.Succeeded)
        {
            Logger.LogInformation("{Name} logged in with {LoginProvider} provider.", externalLoginInfo.Principal.Identity?.Name, externalLoginInfo.LoginProvider);
            RedirectManager.RedirectTo(ReturnUrl);
        }
        else if (result.IsLockedOut)
        {
            RedirectManager.RedirectTo("Account/Lockout");
        }
        else
        {
            RedirectManager.RedirectTo("Account/AccessDenied");
        }
    }

    #endregion // ComponentBase
}