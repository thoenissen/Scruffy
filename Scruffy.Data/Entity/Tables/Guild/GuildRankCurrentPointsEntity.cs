using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Enumerations.Guild;

namespace Scruffy.Data.Entity.Tables.Guild
{
    /// <summary>
    /// Current guild rank points
    /// </summary>
    public class GuildRankCurrentPointsEntity
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
        /// Type
        /// </summary>
        public GuildRankPointType Type { get; set; }

        /// <summary>
        /// Current points
        /// </summary>
        public double Points { get; set; }

        #region Navigation properties

        /// <summary>
        /// Guild
        /// </summary>
        [ForeignKey(nameof(GuildId))]
        public virtual GuildEntity Guild { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}
