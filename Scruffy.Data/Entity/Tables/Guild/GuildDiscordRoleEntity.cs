using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Guild
{
    /// <summary>
    /// Discord roles
    /// </summary>
    [Table("GuildDiscordRoles")]
    public class GuildDiscordRoleEntity
    {
        #region Properties

        /// <summary>
        /// Id of the guild
        /// </summary>
        public long GuildId { get; set; }

        /// <summary>
        /// Id of the role
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// Explanation
        /// </summary>
        public string Explanation { get; set; }

        #region Navigation properties

        /// <summary>
        /// Guild
        /// </summary>
        [ForeignKey(nameof(GuildId))]
        public virtual GuildEntity Guild { get; set; }

        #endregion // Navigation properties

        #endregion // Properties
    }
}