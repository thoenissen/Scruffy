using Scruffy.WebApp.Components.Controls.Abstraction;

namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// Player role
/// </summary>
public class PlayerRoleDTO : IComboBoxEntry
{
    #region Properties

    /// <summary>
    /// Raid role
    /// </summary>
    public RaidRole Role { get; init; }

    /// <summary>
    /// Player
    /// </summary>
    public PlayerDTO Player { get; init; }

    #endregion // Properties

    #region IComboBoxEntry

    /// <inheritdoc />
    public string Group { get; init; }

    /// <inheritdoc />
    public bool IsDisabled => Player.IsAssigned;

    /// <inheritdoc />
    public bool IsEqualsTo(IComboBoxEntry other)
    {
        return other is PlayerRoleDTO otherPlayerRole
               && otherPlayerRole.Player.Id == Player.Id;
    }

    #endregion // IComboBoxEntry

    #region Object

    /// <inheritdoc />
    public override string ToString()
    {
        return Player.Name ?? "Unknown";
    }

    #endregion // Object
}