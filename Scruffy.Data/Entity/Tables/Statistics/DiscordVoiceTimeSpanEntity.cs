using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Statistics
{
    /// <summary>
    /// Discord voice statistics data
    /// </summary>
    [Table("DiscordVoiceTimeSpans")]
    public class DiscordVoiceTimeSpanEntity
    {
        /// <summary>
        /// Id of the server
        /// </summary>
        public ulong ServerId { get; set; }

        /// <summary>
        /// Id of the channel
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Start of the session
        /// </summary>
        public DateTime StartTimeStamp { get; set; }

        /// <summary>
        /// Ending of the session
        /// </summary>
        public DateTime EndTimeStamp { get; set; }

        /// <summary>
        /// Is the voice session completed or ongoing?
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}