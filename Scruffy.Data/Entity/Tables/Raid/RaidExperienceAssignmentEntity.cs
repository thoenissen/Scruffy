using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Assignment of the valid experience level to a configuration
    /// </summary>
    [Table("RaidExperienceAssignments")]
    public class RaidExperienceAssignmentEntity
    {
        #region Properties

        /// <summary>
        /// Id of the configuration
        /// </summary>
        public long ConfigurationId { get; set; }

        /// <summary>
        /// Id of the experience level
        /// </summary>
        public long ExperienceLevelId { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        public long Count { get; set; }

        #region Navigation properties

        /// <summary>
        /// Configuration
        /// </summary>
        [ForeignKey(nameof(ConfigurationId))]
        public virtual RaidDayConfigurationEntity RaidDayConfiguration { get; set; }

        /// <summary>
        /// Configuration
        /// </summary>
        [ForeignKey(nameof(ExperienceLevelId))]
        public virtual RaidExperienceLevelEntity RaidExperienceLevel { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}