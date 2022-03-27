namespace Scruffy.Data.Services.Guild
{
    /// <summary>
    /// Guild rank assignment
    /// </summary>
    public class GuildRankAssignmentData
    {
        /// <summary>
        /// Id of the rank
        /// </summary>
        public int RankId { get; set; }

        /// <summary>
        /// Order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Slots
        /// </summary>
        public int Slots { get; set; }
    }
}
