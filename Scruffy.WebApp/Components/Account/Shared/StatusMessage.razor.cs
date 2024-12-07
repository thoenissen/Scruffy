using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Scruffy.WebApp.Components.Account.Shared;

/// <summary>
/// Status message
/// </summary>
public partial class StatusMessage
{
    #region Fields

    /// <summary>
    /// Message from cookie
    /// </summary>
    private string _messageFromCookie;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// message
    /// </summary>
    [Parameter]
    public string Message { get; set; }

    /// <summary>
    /// Http context
    /// </summary>
    [CascadingParameter]
    private HttpContext HttpContext { get; set; }

    /// <summary>
    /// Display message
    /// </summary>
    private string DisplayMessage => Message ?? _messageFromCookie;

    #endregion // Properties

    #region ComponentBase

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _messageFromCookie = HttpContext.Request.Cookies[IdentityRedirectManager.StatusCookieName];

        if (_messageFromCookie is not null)
        {
            HttpContext.Response.Cookies.Delete(IdentityRedirectManager.StatusCookieName);
        }
    }

    #endregion // ComponentBase
}