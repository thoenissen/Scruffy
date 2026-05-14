using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Upgrades;

/// <summary>
/// Upgrade
/// </summary>
public class Upgrade
{
    #region Properties

    /// <summary>
    /// id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    #endregion // Properties
}