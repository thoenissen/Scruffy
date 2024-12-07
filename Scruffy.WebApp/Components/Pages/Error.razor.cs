using System.Diagnostics;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Scruffy.WebApp.Components.Pages;

/// <summary>
/// Error page
/// </summary>
public partial class Error
{
    #region Properties

    /// <summary>
    /// Http context
    /// </summary>
    [CascadingParameter]
    private HttpContext HttpContext { get; set; }

    /// <summary>
    /// Request ID
    /// </summary>
    private string RequestId { get; set; }

    #endregion // Properties

    #region ComponentBase

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
    }

    #endregion // ComponentBase
}