namespace Scruffy.ServiceHosts.IdentityServer.Pages.Grants;

/// <summary>
/// Grant view model
/// </summary>
public class GrantViewModel
{
    /// <summary>
    /// Client id
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Client name
    /// </summary>
    public string ClientName { get; set; }

    /// <summary>
    /// Client url
    /// </summary>
    public string ClientUrl { get; set; }

    /// <summary>
    /// Client logo url
    /// </summary>
    public string ClientLogoUrl { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Created
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Expires
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Identity grant names
    /// </summary>
    public IEnumerable<string> IdentityGrantNames { get; set; }

    /// <summary>
    /// API grant names
    /// </summary>
    public IEnumerable<string> ApiGrantNames { get; set; }
}