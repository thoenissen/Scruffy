using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Discord
{
    /// <summary>
    /// Blocked discord channel
    /// </summary>
    [Table("BlockedDiscordChannels")]
    public class BlockedDiscordChannelEntity
    {
        #region Properties

        /// <summary>
        /// If of the server
        /// </summary>
        public ulong ServerId { get; set; }

        /// <summary>
        /// Id of the channel
        /// </summary>
        public ulong ChannelId { get; set; }

        #endregion // Properties
    }
}
