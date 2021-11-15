using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Raid role
/// </summary>
[Table("RaidRoles")]
public class RaidRoleEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Id of the main role
    /// </summary>
    public long? MainRoleId { get; set; }

    /// <summary>
    /// Discord emoji
    /// </summary>
    public ulong DiscordEmojiId { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Is the role deleted?
    /// </summary>
    public bool IsDeleted { get; set; }

    #region Navigation properties

    /// <summary>
    /// Main role
    /// </summary>
    [ForeignKey(nameof(MainRoleId))]
    public virtual RaidRoleEntity MainRaidRole { get; set; }

    /// <summary>
    /// Sub roles
    /// </summary>
    public virtual ICollection<RaidRoleEntity> SubRaidRoles { get; set; }

    /// <summary>
    /// Lineup entries
    /// </summary>
    public virtual ICollection<RaidRoleLineupEntryEntity> RaidRoleLineupEntries { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}