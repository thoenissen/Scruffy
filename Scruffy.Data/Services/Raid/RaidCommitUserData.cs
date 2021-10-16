namespace Scruffy.Data.Services.Raid
{
    /// <summary>
    /// User commit data
    /// </summary>
    public class RaidCommitUserData
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong DiscordUserId { get; set; }

        /// <summary>
        /// Points
        /// </summary>
        public double Points { get; set; }

        /// <summary>
        /// Emoji
        /// </summary>
        public ulong DiscordEmoji { get; set; }
    }
}