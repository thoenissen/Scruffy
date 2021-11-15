using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Achievements;

/// <summary>
/// Achievement
/// </summary>
public class Achievement
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Icon
    /// </summary>
    [JsonProperty("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Requirement
    /// </summary>
    [JsonProperty("requirement")]
    public string Requirement { get; set; }

    /// <summary>
    /// Locked text
    /// </summary>
    [JsonProperty("locked_text")]
    public string LockedText { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Flags
    /// </summary>
    [JsonProperty("flags")]
    public List<string> Flags { get; set; }

    /// <summary>
    /// Tiers
    /// </summary>
    [JsonProperty("tiers")]
    public List<Tier> Tiers { get; set; }

    /// <summary>
    /// Prerequisites
    /// </summary>
    [JsonProperty("prerequisites")]
    public List<int> Prerequisites { get; set; }

    /// <summary>
    /// Rewards
    /// </summary>
    [JsonProperty("rewards")]
    public List<Reward> Rewards { get; set; }

    /// <summary>
    /// Bits
    /// </summary>
    [JsonProperty("bits")]
    public List<Bit> Bits { get; set; }

    /// <summary>
    /// Point cap
    /// </summary>
    [JsonProperty("point_cap")]
    public int? PointCap { get; set; }
}