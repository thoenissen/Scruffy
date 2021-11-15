using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Role lineup header
/// </summary>
[Table("RaidRoleLineupHeaders")]
public class RaidRoleLineupHeaderEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    #region Navigation properties

    /// <summary>
    /// Lineups
    /// </summary>
    public virtual ICollection<RaidRoleLineupAssignmentEntity> RaidRoleLineupAssignments { get; set; }

    /// <summary>
    /// Entries
    /// </summary>
    public virtual ICollection<RaidRoleLineupEntryEntity> RaidRoleLineupEntries { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}