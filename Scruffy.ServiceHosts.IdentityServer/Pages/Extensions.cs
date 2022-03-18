using Duende.IdentityServer.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages;

/// <summary>
/// Extension methods
/// </summary>
public static class Extensions
{
    #region Methods

    /// <summary>
    /// Determines if the authentication scheme support signout.
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="scheme">Scheme</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task<bool> GetSchemeSupportsSignOutAsync(this HttpContext context, string scheme)
    {
        var provider = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
        var handler = await provider.GetHandlerAsync(context, scheme)
                                    .ConfigureAwait(false);

        return handler is IAuthenticationSignOutHandler;
    }

    /// <summary>
    /// Checks if the redirect URI is for a native client.
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>true: Native client</returns>
    public static bool IsNativeClient(this AuthorizationRequest context)
    {
        return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
            && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
    }

    /// <summary>
    /// Renders a loading page that is used to redirect back to the redirectUri.
    /// </summary>
    /// <param name="page">Page</param>
    /// <param name="redirectUri">Redirect uri</param>
    /// <returns>Action result</returns>
    public static IActionResult LoadingPage(this PageModel page, string redirectUri)
    {
        page.HttpContext.Response.StatusCode = 200;
        page.HttpContext.Response.Headers["Location"] = string.Empty;

        return page.RedirectToPage("/Redirect/Index", new { RedirectUri = redirectUri });
    }

    #endregion // Methods
}