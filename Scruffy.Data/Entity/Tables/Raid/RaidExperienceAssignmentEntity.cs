using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Assignment of the valid experience level to a template
    /// </summary>
    [Table("RaidExperienceAssignments")]
    public class RaidExperienceAssignmentEntity
    {
        #region Properties

        /// <summary>
        /// Id of the configuration
        /// </summary>
        public long TemplateId { get; set; }

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
        /// Template
        /// </summary>
        [ForeignKey(nameof(TemplateId))]
        public virtual RaidDayTemplateEntity RaidDayTemplateEntity { get; set; }

        /// <summary>
        /// Configuration
        /// </summary>
        [ForeignKey(nameof(ExperienceLevelId))]
        public virtual RaidExperienceLevelEntity RaidExperienceLevel { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}