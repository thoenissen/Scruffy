using Scruffy.Data.Enumerations.Raid;

namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// Player
/// </summary>
public class PlayerDTO
{
    #region Properties

    /// <summary>
    /// User ID
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Roles
    /// </summary>
    public RaidRole Roles { get; init; }

    /// <summary>
    /// Roles of the registration
    /// </summary>
    public RaidRole RegistrationRoles { get; init; }

    /// <summary>
    /// Is the player assigned?
    /// </summary>
    public bool IsAssigned { get; set; }

    /// <summary>
    /// Discord account ID
    /// </summary>
    public ulong DiscordAccountId { get; init; }

    /// <summary>
    /// Is the player on the substitutes' bench?
    /// </summary>
    public bool IsOnSubstitutesBench { get; init; }

    #endregion // Properties

    #region Object

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion // Object
}