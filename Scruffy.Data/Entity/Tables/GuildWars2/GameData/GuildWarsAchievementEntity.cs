using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData;

/// <summary>
/// Achievement
/// </summary>
[Table("GuildWarsAchievements")]
public class GuildWarsAchievementEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    /// <summary>
    /// Icon
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Requirement
    /// </summary>
    public string Requirement { get; set; }

    /// <summary>
    /// Locked text
    /// </summary>
    public string LockedText { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Point cap
    /// </summary>
    public int? PointCap { get; set; }

    #region Navigation properties

    /// <summary>
    /// Flags
    /// </summary>
    public virtual ICollection<GuildWarsAchievementFlagEntity> GuildWarsAchievementFlags { get; set; }

    /// <summary>
    /// Prerequisites
    /// </summary>
    public virtual ICollection<GuildWarsAchievementPrerequisiteEntity> GuildWarsAchievementPrerequisites { get; set; }

    /// <summary>
    /// Bits
    /// </summary>
    public virtual ICollection<GuildWarsAchievementBitEntity> GuildWarsAchievementBits { get; set; }

    /// <summary>
    /// Rewards
    /// </summary>
    public virtual ICollection<GuildWarsAchievementRewardEntity> GuildWarsAchievementRewards { get; set; }

    /// <summary>
    /// Tiers
    /// </summary>
    public virtual ICollection<GuildWarsAchievementTierEntity> GuildWarsAchievementTiers { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}