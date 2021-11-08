using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData
{
    /// <summary>
    /// Reward
    /// </summary>
    [Table("GuildWarsAchievementRewards")]
    public class GuildWarsAchievementRewardEntity
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
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Region
        /// </summary>
        public string Region { get; set; }

        #region Navigation properties

        /// <summary>
        /// Achievement
        /// </summary>
        [ForeignKey(nameof(AchievementId))]
        public virtual GuildWarsAchievementEntity GuildWarsAchievement { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}