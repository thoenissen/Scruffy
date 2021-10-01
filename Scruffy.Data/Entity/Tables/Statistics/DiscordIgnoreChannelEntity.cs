using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Statistics
{
    /// <summary>
    /// Discord channel ignore list
    /// </summary>
    [Table("DiscordIgnoreChannels")]
    public class DiscordIgnoreChannelEntity
    {
        /// <summary>
        /// Id of the server
        /// </summary>
        public ulong ServerId { get; set; }

        /// <summary>
        /// Id of the channel
        /// </summary>
        public ulong ChannelId { get; set; }
    }
}