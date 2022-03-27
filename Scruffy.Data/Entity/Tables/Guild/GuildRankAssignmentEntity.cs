using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Guild
{
    /// <summary>
    /// Guild rank assignment
    /// </summary>
    [Table("GuildRankAssignments")]
    public class GuildRankAssignmentEntity
    {
        #region Properties

        /// <summary>
        /// Id of the guild
        /// </summary>
        public long GuildId { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Id of the rank
        /// </summary>
        public int RankId { get; set; }

        /// <summary>
        /// Time stamp
        /// </summary>
        public DateTime TimeStamp { get; set; }

        #region Navigation properties

        /// <summary>
        /// Guild
        /// </summary>
        [ForeignKey(nameof(GuildId))]
        public virtual GuildEntity Guid { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        /// <summary>
        /// Rank
        /// </summary>
        [ForeignKey(nameof(RankId))]
        public virtual GuildRankEntity Rank { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
