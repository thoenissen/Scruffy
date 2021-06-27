using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Experience level
    /// </summary>
    [Table("RaidExperienceLevels")]
    public class RaidExperienceLevelEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Superior experience level
        /// </summary>
        public long? SuperiorExperienceLevelId { get; set; }

        /// <summary>
        /// Discord role
        /// </summary>
        public ulong? DiscordRoleId { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Discord emoji
        /// </summary>
        public ulong DiscordEmoji { get; set; }

        /// <summary>
        /// Is the level deleted?
        /// </summary>
        public bool IsDeleted { get; set; }

        #region Navigation properties

        /// <summary>
        /// Superior raid level
        /// </summary>
        [ForeignKey(nameof(SuperiorExperienceLevelId))]
        public virtual RaidExperienceLevelEntity SuperiorRaidExperienceLevel { get; set; }

        /// <summary>
        /// Inferior raid levels
        /// </summary>
        public virtual ICollection<RaidExperienceLevelEntity> InferiorRaidExperienceLevels { get; set; }

        /// <summary>
        /// Assignments
        /// </summary>
        public virtual ICollection<RaidExperienceAssignmentEntity> RaidExperienceAssignments { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}