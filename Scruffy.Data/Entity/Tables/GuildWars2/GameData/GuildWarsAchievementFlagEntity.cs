using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData;

/// <summary>
/// Flag
/// </summary>
[Table("GuildWarsAchievementFlags")]
public class GuildWarsAchievementFlagEntity
{
    #region Properties

    /// <summary>
    /// Id of the achievement
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// Flag
    /// </summary>
    [StringLength(50)]
    public string Flag { get; set; }

    #region Navigation properties

    /// <summary>
    /// Achievement
    /// </summary>
    [ForeignKey(nameof(AchievementId))]
    public virtual GuildWarsAchievementEntity GuildWarsAchievement { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}