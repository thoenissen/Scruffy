using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Characters;

/// <summary>
/// Character
/// </summary>
public class Character
{
    /// <summary>
    /// Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Race
    /// </summary>
    [JsonProperty("race")]
    public string Race { get; set; }

    /// <summary>
    /// Gender
    /// </summary>
    [JsonProperty("gender")]
    public string Gender { get; set; }

    /// <summary>
    /// Flags
    /// </summary>
    [JsonProperty("flags")]
    public List<string> Flags { get; set; }

    /// <summary>
    /// Profession
    /// </summary>
    [JsonProperty("profession")]
    public string Profession { get; set; }

    /// <summary>
    /// Level
    /// </summary>
    [JsonProperty("level")]
    public int Level { get; set; }

    /// <summary>
    /// Guild
    /// </summary>
    [JsonProperty("guild")]
    public string Guild { get; set; }

    /// <summary>
    /// Age
    /// </summary>
    [JsonProperty("age")]
    public int Age { get; set; }

    /// <summary>
    /// Created
    /// </summary>
    [JsonProperty("created")]
    public DateTime Created { get; set; }

    /// <summary>
    /// Deaths
    /// </summary>
    [JsonProperty("deaths")]
    public int Deaths { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    [JsonProperty("title")]
    public int Title { get; set; }
}