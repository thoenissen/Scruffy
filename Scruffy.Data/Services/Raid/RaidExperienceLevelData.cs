namespace Scruffy.Data.Services.Raid
{
    /// <summary>
    /// Experience level
    /// </summary>
    public class RaidExperienceLevelData
    {
        #region Properties

        /// <summary>
        /// Superior experience level id
        /// </summary>
        public long? SuperiorExperienceLevelId { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Discord emoji
        /// </summary>
        public ulong DiscordEmoji { get; set; }

        #endregion // Properties
    }
}
