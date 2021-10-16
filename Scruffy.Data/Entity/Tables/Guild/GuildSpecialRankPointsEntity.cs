using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Guild
{
    /// <summary>
    /// Current special rank points of a user
    /// </summary>
    [Table("GuildSpecialRankPoints")]
    public class GuildSpecialRankPointsEntity
    {
        #region Properties

        /// <summary>
        /// Id of the configuration
        /// </summary>
        public long ConfigurationId { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Current points
        /// </summary>
        public double Points { get; set; }

        #region Navigation properties

        /// <summary>
        /// Configuration
        /// </summary>
        [ForeignKey(nameof(ConfigurationId))]
        public virtual GuildSpecialRankConfigurationEntity GuildSpecialRankConfiguration { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}