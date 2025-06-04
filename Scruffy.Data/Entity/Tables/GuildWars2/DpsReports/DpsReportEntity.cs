using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Enumerations.GuildWars2;

namespace Scruffy.Data.Entity.Tables.GuildWars2.DpsReports
{
    /// <summary>
    /// dps.report reports
    /// </summary>
    [Table("DpsReports")]
    public class DpsReportEntity
    {
        #region Properties

        /// <summary>
        /// User id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [StringLength(64)]
        public string Id { get; set; }

        /// <summary>
        /// Perma link
        /// </summary>
        [StringLength(64)]
        public string PermaLink { get; set; }

        /// <summary>
        /// Upload time
        /// </summary>
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// Encounter time
        /// </summary>
        public DateTime EncounterTime { get; set; }

        /// <summary>
        /// Id of the boss
        /// </summary>
        public long BossId { get; set; }

        /// <summary>
        /// Success
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Mode
        /// </summary>
        public DpsReportMode Mode { get; set; }

        /// <summary>
        /// State of the processing
        /// </summary>
        public DpsReportProcessingState State { get; set; }

        #region Navigation properties

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}