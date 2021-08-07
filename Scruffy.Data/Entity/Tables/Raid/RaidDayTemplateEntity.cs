using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Raid day template
    /// </summary>
    [Table("RaidDayTemplates")]
    public class RaidDayTemplateEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Alias name
        /// </summary>
        [StringLength(20)]
        public string AliasName { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Thumbnail
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// Ist the template deleted?
        /// </summary>
        public bool IsDeleted { get; set; }

        #region Navigation properties

        /// <summary>
        /// Appointments
        /// </summary>
        public virtual ICollection<RaidAppointmentEntity> RaidAppointments { get; set; }

        /// <summary>
        /// Experience level assignments
        /// </summary>
        public virtual ICollection<RaidExperienceAssignmentEntity> RaidExperienceAssignments { get; set; }

        /// <summary>
        /// Lineups
        /// </summary>
        public virtual ICollection<RaidRoleLineupAssignmentEntity> RaidRoleLineupAssignments { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
