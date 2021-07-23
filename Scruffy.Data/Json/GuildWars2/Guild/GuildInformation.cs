using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild
{
    /// <summary>
    /// Guild information
    /// </summary>
    public class GuildInformation
    {
        /// <summary>
        /// Level
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        /// The message of the day written out in a single string.
        /// </summary>
        [JsonProperty("motd")]
        public string MessageOfTheDay { get; set; }

        /// <summary>
        /// The guild's current influence.
        /// </summary>
        [JsonProperty("influence")]
        public int Influence { get; set; }

        /// <summary>
        /// The guild's current aetherium level.
        /// </summary>
        [JsonProperty("aetherium")]
        public int Aetherium { get; set; }

        /// <summary>
        /// The guild's current level of resonance.
        /// </summary>
        [JsonProperty("resonance")]
        public int Resonance { get; set; }

        /// <summary>
        /// The guild's current level of favor.
        /// </summary>
        [JsonProperty("favor")]
        public int Favor { get; set; }

        /// <summary>
        ///  The number of People currently in the Guild.
        /// </summary>
        [JsonProperty("member_count")]
        public int MemberCount { get; set; }

        /// <summary>
        /// The maximum number of People that can be in the Guild.
        /// </summary>
        [JsonProperty("member_capacity")]
        public int MemberCapacity { get; set; }

        /// <summary>
        /// The unique guild id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The guild's name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The 2 to 4 letter guild tag representing the guild.
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; }

        /// <summary>
        /// The guild emblem
        /// </summary>
        [JsonProperty("emblem")]
        public GuildEmblem Emblem { get; set; }
    }
}