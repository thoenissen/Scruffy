using Duende.IdentityServer.Models;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Home.Error;

/// <summary>
/// Error view model
/// </summary>
public class ViewModel
{
    #region Properties

    /// <summary>
    /// Error message
    /// </summary>
    public ErrorMessage Error { get; set; }

    #endregion // Properties
}