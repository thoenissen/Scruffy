using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Scruffy.WebApp.Components.Account;

/// <summary>
/// Redirect manager
/// </summary>
internal sealed class IdentityRedirectManager
{
    #region Constants

    /// <summary>
    /// Name of the status cookie
    /// </summary>
    public const string StatusCookieName = "Identity.StatusMessage";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Status cookie builder
    /// </summary>
    private static readonly CookieBuilder _statusCookieBuilder = new()
                                                                 {
                                                                     SameSite = SameSiteMode.Strict,
                                                                     HttpOnly = true,
                                                                     IsEssential = true,
                                                                     MaxAge = TimeSpan.FromSeconds(5),
                                                                 };

    /// <summary>
    /// Navigation manager
    /// </summary>
    private readonly NavigationManager _navigationManager;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Redirect manager
    /// </summary>
    /// <param name="navigationManager">Navigation manager</param>
    public IdentityRedirectManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Redirect to uri
    /// </summary>
    /// <param name="uri">Uri</param>
    [DoesNotReturn]
    public void RedirectTo(string uri)
    {
        uri ??= string.Empty;

        // Prevent open redirects.
        if (Uri.IsWellFormedUriString(uri, UriKind.Relative) == false)
        {
            uri = _navigationManager.ToBaseRelativePath(uri);
        }

        // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
        // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
        _navigationManager.NavigateTo(uri);

        throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
    }

    /// <summary>
    /// Redirect  to uri with status
    /// </summary>
    /// <param name="uri">Uri</param>
    /// <param name="message">Message</param>
    /// <param name="context">Context</param>
    [DoesNotReturn]
    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookieName, message, _statusCookieBuilder.Build(context));

        RedirectTo(uri);
    }

    #endregion // Methods
}