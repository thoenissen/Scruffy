using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Role lineup entry
/// </summary>
[Table("RaidRoleLineupEntries")]
public class RaidRoleLineupEntryEntity
{
    #region Properties

    /// <summary>
    /// Id of the header
    /// </summary>
    public long LineupHeaderId { get; set; }

    /// <summary>
    /// Lineup position
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// Id of the role
    /// </summary>
    public long RoleId { get; set; }

    #region Navigation properties

    /// <summary>
    /// Lineup header
    /// </summary>
    [ForeignKey(nameof(LineupHeaderId))]
    public RaidRoleLineupHeaderEntity RaidRoleLineupHeader { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    [ForeignKey(nameof(RoleId))]
    public RaidRoleEntity RaidRole { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}