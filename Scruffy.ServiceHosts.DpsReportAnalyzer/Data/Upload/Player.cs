using Newtonsoft.Json;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;

/// <summary>
/// Player
/// </summary>
public class Player
{
    /// <summary>
    /// Display name
    /// </summary>
    [JsonProperty("display_name")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Character name
    /// </summary>
    [JsonProperty("character_name")]
    public string CharacterName { get; set; }

    /// <summary>
    /// Profession
    /// </summary>
    [JsonProperty("profession")]
    public int Profession { get; set; }

    /// <summary>
    /// Elite spec
    /// </summary>
    [JsonProperty("elite_spec")]
    public int EliteSpec { get; set; }
}