using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Account
{
    /// <summary>
    /// Account achievement
    /// </summary>
    public class AccountAchievement
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Bits
        /// </summary>
        [JsonProperty("bits")]
        public List<int> Bits { get; set; }

        /// <summary>
        /// Current progress
        /// </summary>
        [JsonProperty("current")]
        public int? Current { get; set; }

        /// <summary>
        /// Maximum progress
        /// </summary>
        [JsonProperty("max")]
        public int? Maximum { get; set; }

        /// <summary>
        /// Is the achievement done?
        /// </summary>
        [JsonProperty("done")]
        public bool IsDone { get; set; }

        /// <summary>
        /// Count of repetition
        /// </summary>
        [JsonProperty("repeated")]
        public int? RepetitionCount { get; set; }

        /// <summary>
        /// Is the achievement unlocked?
        /// </summary>
        [JsonProperty("unlocked")]
        public bool? IsUnlocked { get; set; }
    }
}
