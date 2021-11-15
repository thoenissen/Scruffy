using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData;

/// <summary>
/// Prerequisites
/// </summary>
[Table("GuildWarsAchievementPrerequisites")]
public class GuildWarsAchievementPrerequisiteEntity
{
    #region Properties

    /// <summary>
    /// Id of the achievement
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    #region Navigation properties

    /// <summary>
    /// Achievement
    /// </summary>
    [ForeignKey(nameof(AchievementId))]
    public virtual GuildWarsAchievementEntity GuildWarsAchievement { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}