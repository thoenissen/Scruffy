using Duende.IdentityServer.Models;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Home.Error;

/// <summary>
/// View model
/// </summary>
public class ViewModel
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public ViewModel()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="error">Error</param>
    public ViewModel(string error)
    {
        Error = new ErrorMessage { Error = error };
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Error
    /// </summary>
    public ErrorMessage Error { get; set; }

    #endregion // Properties
}