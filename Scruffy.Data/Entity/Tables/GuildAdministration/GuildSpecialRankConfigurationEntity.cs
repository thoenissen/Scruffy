using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildAdministration
{
    /// <summary>
    /// Special rank configuration
    /// </summary>
    [Table("GuildSpecialRankConfigurations")]
    public class GuildSpecialRankConfigurationEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Id of the guild
        /// </summary>
        public long GuildId { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Id of the discord role
        /// </summary>
        public ulong DiscordRoleId { get; set; }

        /// <summary>
        /// Maximum points
        /// </summary>
        public double MaximumPoints { get; set; }

        /// <summary>
        /// Grand role threshold
        /// </summary>
        public double GrantThreshold { get; set; }

        /// <summary>
        /// Remove role threshold
        /// </summary>
        public double RemoveThreshold { get; set; }

        /// <summary>
        /// Deletion flag
        /// </summary>
        public bool IsDeleted { get; set; }

        #region Navigation properties

        /// <summary>
        /// Guild
        /// </summary>
        [ForeignKey(nameof(GuildId))]
        public virtual GuildEntity Guild { get; set; }

        /// <summary>
        /// Points
        /// </summary>
        public virtual ICollection<GuildSpecialRankPointsEntity> GuildSpecialRankPoints { get; set; }

        /// <summary>
        /// Protocol
        /// </summary>
        public virtual ICollection<GuildSpecialRankProtocolEntryEntity> GuildSpecialRankProtocolEntries { get; set; }

        /// <summary>
        /// Assignments
        /// </summary>
        public virtual ICollection<GuildSpecialRankRoleAssignmentEntity> GuildSpecialRankRoleAssignments { get; set; }

        /// <summary>
        /// Ignore roles
        /// </summary>
        public virtual ICollection<GuildSpecialRankIgnoreRoleAssignmentEntity> GuildSpecialRankIgnoreRoleAssignments { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
