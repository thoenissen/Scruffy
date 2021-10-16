using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Guild
{
    /// <summary>
    /// Guilds
    /// </summary>
    [Table("Guilds")]
    public class GuildEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// API-Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Id of the guild
        /// </summary>
        public string GuildId { get; set; }

        /// <summary>
        /// Discord Server
        /// </summary>
        public ulong DiscordServerId { get; set; }

        #region Navigation - Properties

        /// <summary>
        /// Discord server
        /// </summary>
        [ForeignKey(nameof(DiscordServerId))]
        public virtual ServerConfigurationEntity ServerConfiguration { get; set; }

        /// <summary>
        /// Log entries
        /// </summary>
        public virtual ICollection<GuildLogEntryEntity> GuildLogEntries { get; set; }

        #endregion // Navigation - Properties

        #endregion // Properties
    }
}
