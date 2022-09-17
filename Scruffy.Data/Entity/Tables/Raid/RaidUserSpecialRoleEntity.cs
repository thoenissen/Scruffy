using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Special role of an user
    /// </summary>
    [Table("RaidUserSpecialRoles")]
    public class RaidUserSpecialRoleEntity
    {
        #region Properties

        /// <summary>
        /// Id of the user
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Id of the role
        /// </summary>
        public long SpecialRoleId { get; set; }

        #region Navigation properties

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        [ForeignKey(nameof(SpecialRoleId))]
        public virtual RaidSpecialRoleEntity RaidSpecialRole { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}