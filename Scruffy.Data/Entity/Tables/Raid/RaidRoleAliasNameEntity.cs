using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid
{
    /// <summary>
    /// Alias name of a role combination
    /// </summary>
    [Table("RaidRoleAliasNames")]
    public class RaidRoleAliasNameEntity
    {
        #region Properties

        /// <summary>
        /// Alias name
        /// </summary>
        [Key]
        [StringLength(20)]
        public string AliasName { get; set; }

        /// <summary>
        /// Id of the main role
        /// </summary>
        public long MainRoleId { get; set; }

        /// <summary>
        /// Id of the sub role
        /// </summary>
        public long? SubRoleId { get; set; }

        #region Navigation properties

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