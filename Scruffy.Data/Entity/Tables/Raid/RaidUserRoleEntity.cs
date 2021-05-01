using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Role of an user
    /// </summary>
    [Table("RaidUserRoles")]
    public class RaidUserRoleEntity
    {
        #region Properties

        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Id of the main role
        /// </summary>
        public long MainRoleId { get; set; }

        /// <summary>
        /// Id of the sub role
        /// </summary>
        public long SubRoleId { get; set; }

        #region Navigation properties

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        /// <summary>
        /// Main role
        /// </summary>
        [ForeignKey(nameof(MainRoleId))]
        public virtual RaidRoleEntity MainRaidRole { get; set; }

        /// <summary>
        /// Sub role
        /// </summary>
        [ForeignKey(nameof(SubRoleId))]
        public virtual RaidRoleEntity SubRaidRole { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}