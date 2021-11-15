using System;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild;

/// <summary>
/// Guild member
/// </summary>
public class GuildMember
{
    /// <summary>
    /// Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Rank
    /// </summary>
    [JsonProperty("rank")]
    public string Rank { get; set; }

    /// <summary>
    /// Joined
    /// </summary>
    [JsonProperty("joined")]
    public DateTime? Joined { get; set; }
}