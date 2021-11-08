using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData
{
    /// <summary>
    /// Bit
    /// </summary>
    [Table("GuildWarsAchievementBits")]
    public class GuildWarsAchievementBitEntity
    {
        #region Properties

        /// <summary>
        /// Id of the achievement
        /// </summary>
        public int AchievementId { get; set; }

        /// <summary>
        /// Bit
        /// </summary>
        public int Bit { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }

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