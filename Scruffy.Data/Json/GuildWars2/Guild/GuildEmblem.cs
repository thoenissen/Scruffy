using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild;

/// <summary>
/// Guild emblem
/// </summary>
public class GuildEmblem
{
    /// <summary>
    /// An array containing information of the background of the guild emblem.
    /// </summary>
    [JsonProperty("background")]
    public GuildEmblemBackground Background { get; set; }

    /// <summary>
    ///  An array containing information of the foreground of the guild emblem.
    /// </summary>
    [JsonProperty("foreground")]
    public GuildEmblemForeground Foreground { get; set; }

    /// <summary>
    /// An array containing the manipulations done to the logo. Possible values:
    ///  - FlipBackgroundHorizontal
    ///  - FlipBackgroundVertical
    /// </summary>
    [JsonProperty("flags")]
    public List<string> Flags { get; set; }
}