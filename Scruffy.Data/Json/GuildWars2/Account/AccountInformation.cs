using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Account
{
    /// <summary>
    /// Information about the player account
    /// </summary>
    public class AccountInformation
    {
        /// <summary>
        /// The unique persistent account GUID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The unique account name with numerical suffix. It is possible that the name change. Do not rely on the name, use id instead.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The age of the account in seconds.
        /// </summary>
        [JsonProperty("age")]
        public long Age { get; set; }

        /// <summary>
        /// The id of the home world the account is assigned to.
        /// </summary>
        [JsonProperty("world")]
        public long World { get; set; }

        /// <summary>
        ///  A list of guilds assigned to the given account.
        /// </summary>
        [JsonProperty("guilds")]
        public List<string> Guilds { get; set; }

        /// <summary>
        /// A list of guilds the account is leader of. Requires the additional guilds scope.
        /// </summary>
        [JsonProperty("guild_leader")]
        public List<string> GuildLeader { get; set; }

        /// <summary>
        /// An ISO-8601 standard timestamp of when the account was created.
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// A list of what content this account has access to. Possible values:
        ///     None – should probably never happen
        ///     PlayForFree – has not yet purchased the game
        ///     GuildWars2 – has purchased the base game
        ///     HeartOfThorns – has purchased Heart of Thorns
        ///     PathOfFire – has purchased Path of Fire
        /// </summary>
        [JsonProperty("access")]
        public List<string> Access { get; set; }

        /// <summary>
        /// True if the player has bought a commander tag.
        /// </summary>
        [JsonProperty("commander")]
        public bool Commander { get; set; }

        /// <summary>
        /// The account's personal fractal reward level. Requires the additional progression scope.
        /// </summary>
        [JsonProperty("fractal_level")]
        public int FractalLevel { get; set; }

        /// <summary>
        /// The daily AP the account has. Requires the additional progression scope.
        /// </summary>
        [JsonProperty("daily_ap")]
        public int DailyAchievementPoints { get; set; }

        /// <summary>
        /// The monthly AP the account has. Requires the additional progression scope.
        /// </summary>
        [JsonProperty("monthly_ap")]
        public int MonthlyAchievementPoints { get; set; }

        /// <summary>
        /// The account's personal wvw rank. Requires the additional progression scope.
        /// </summary>
        [JsonProperty("wvw_rank")]
        public int WvWRank { get; set; }

        /// <summary>
        /// The number of build storage slots available on the account.
        /// </summary>
        [JsonProperty("build_storage_slots")]
        public int BuildStorageSlots { get; set; }

        /// <summary>
        /// An ISO-8601 standard timestamp of when the account information last changed as perceived by the API.
        /// </summary>
        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }
    }
}
