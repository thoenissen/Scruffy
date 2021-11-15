using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild;

/// <summary>
/// Layers of a guild emblem foreground or background
/// </summary>
public class GuildEmblemLayerData
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Layers
    /// </summary>
    [JsonProperty("layers")]
    public List<string> Layers { get; set; }
}