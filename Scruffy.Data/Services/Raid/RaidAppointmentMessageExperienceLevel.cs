namespace Scruffy.Data.Services.Raid
{
    /// <summary>
    /// Experience level data
    /// </summary>
    public class RaidAppointmentMessageExperienceLevel
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Emoji
        /// </summary>
        public ulong DiscordEmoji { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        public long Count { get; set; }
    }
}
