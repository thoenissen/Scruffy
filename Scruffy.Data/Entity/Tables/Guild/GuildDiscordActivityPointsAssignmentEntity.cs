using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Enumerations.Guild;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Configuration of additional points cause of discord activity
/// </summary>
[Table("GuildDiscordActivityPointsAssignments")]
public class GuildDiscordActivityPointsAssignmentEntity
{
    #region Properties

    /// <summary>
    /// Id of the guild
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Typ
    /// </summary>
    public DiscordActivityPointsType Type { get; set; }

    /// <summary>
    /// Id of the discord role
    /// </summary>
    public ulong RoleId { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    public double Points { get; set; }

    #region Navigation properties

    /// <summary>
    /// Guild
    /// </summary>
    [ForeignKey(nameof(GuildId))]
    public virtual GuildEntity Guild { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}