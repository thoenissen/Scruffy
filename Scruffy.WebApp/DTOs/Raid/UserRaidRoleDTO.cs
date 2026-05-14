using Scruffy.Data.Enumerations.Raid;

namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// User raid role
/// </summary>
public class UserRaidRoleDTO
{
    #region Properties

    /// <summary>
    /// User name
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Roles
    /// </summary>
    public RaidRole Roles { get; init; }

    /// <summary>
    /// Special roles
    /// </summary>
    public RaidSpecialRole SpecialRoles { get; init; }

    /// <summary>
    /// Is the player currently active?
    /// </summary>
    public bool IsActive { get; set; }

    #endregion // Properties
}