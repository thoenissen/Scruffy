using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Guild
{
    /// <summary>
    /// Assigning roles to the configuration
    /// </summary>
    [Table("GuildSpecialRankRoleAssignments")]
    public class GuildSpecialRankRoleAssignmentEntity
    {
        #region Properties

        /// <summary>
        /// Id of the configuration
        /// </summary>
        public long ConfigurationId { get; set; }

        /// <summary>
        /// Id of the discord role
        /// </summary>
        public ulong DiscordRoleId { get; set; }

        /// <summary>
        /// Points
        /// </summary>
        public double Points { get; set; }

        #region Navigation properties

        /// <summary>
        /// Configuration
        /// </summary>
        [ForeignKey(nameof(ConfigurationId))]
        public virtual GuildSpecialRankConfigurationEntity GuildSpecialRankConfiguration { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}