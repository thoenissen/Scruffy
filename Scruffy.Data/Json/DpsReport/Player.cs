using Newtonsoft.Json;

namespace Scruffy.Data.Json.DpsReport;

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
    /// Display name from detailed logs
    /// </summary>
    [JsonProperty("account")]
    private string Account
    {
        set => DisplayName = value;
    }

    /// <summary>
    /// Character name
    /// </summary>
    [JsonProperty("character_name")]
    public string CharacterName { get; set; }

    /// <summary>
    /// Character name from detailed logs
    /// </summary>
    [JsonProperty("name")]
    private string Name
    {
        set => CharacterName = value;
    }
}