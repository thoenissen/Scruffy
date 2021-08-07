using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Assigning lineups to a template
    /// </summary>
    [Table("RaidRoleLineupAssignments")]
    public class RaidRoleLineupAssignmentEntity
    {
        #region Properties

        /// <summary>
        /// Id of the template
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// Id of the lineup
        /// </summary>
        public long LineupHeaderId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Template
        /// </summary>
        [ForeignKey(nameof(TemplateId))]
        public virtual RaidDayTemplateEntity RaidDayTemplate { get; set; }

        /// <summary>
        /// Lineup
        /// </summary>
        [ForeignKey(nameof(LineupHeaderId))]
        public virtual RaidRoleLineupHeaderEntity RaidRoleLineupHeader { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}