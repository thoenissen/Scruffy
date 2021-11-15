using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData;

/// <summary>
/// Tier
/// </summary>
[Table("GuildWarsAchievementTiers")]
public class GuildWarsAchievementTierEntity
{
    #region Properties

    /// <summary>
    /// Id of the achievement
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// Counter
    /// </summary>
    public int Counter { get; set; }

    /// <summary>
    /// Count
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    public int Points { get; set; }

    #region Navigation properties

    /// <summary>
    /// Achievement
    /// </summary>
    [ForeignKey(nameof(AchievementId))]
    public virtual GuildWarsAchievementEntity GuildWarsAchievement { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}