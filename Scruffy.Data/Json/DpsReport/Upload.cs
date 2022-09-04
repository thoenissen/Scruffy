using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Upload
/// </summary>
public class Upload
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Permalink
    /// </summary>
    [JsonProperty("permalink")]
    public string Permalink { get; set; }

    /// <summary>
    /// Upload time
    /// </summary>
    [JsonProperty("uploadTime")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime UploadTime { get; set; }

    /// <summary>
    /// Encounter time
    /// </summary>
    [JsonProperty("encounterTime")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime EncounterTime { get; set; }

#nullable enable
    /// <summary>
    /// Language
    /// </summary>
    [JsonProperty("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Language id
    /// </summary>
    [JsonProperty("languageId")]
    public int? LanguageId { get; set; }
#nullable disable

    /// <summary>
    /// Players
    /// </summary>
    [JsonProperty("players")]
    public Dictionary<string, Player> Players { get; set; }

    /// <summary>
    /// Encounter
    /// </summary>
    [JsonProperty("encounter")]
    public Encounter Encounter { get; set; }

    /// <summary>
    /// Report
    /// </summary>
    [JsonProperty("report")]
    public Report Report { get; set; }

    /// <summary>
    /// HashSet of all player's unique names
    /// </summary>
    public HashSet<string> Group => Players.Select(obj => obj.Value.DisplayName).Distinct().ToHashSet();

    /// <inheritdoc/>
    public override bool Equals(object otherObj)
    {
        if (otherObj is Upload other)
        {
            if (Math.Abs((EncounterTime - other.EncounterTime).TotalSeconds) < 5.0)
            {
                // Voice & Claw + Statues of Darkness special treatment
                if (Encounter.BossId == other.Encounter.BossId
                    || (Math.Min(Encounter.BossId, other.Encounter.BossId) == 22343 && Math.Max(Encounter.BossId, other.Encounter.BossId) == 22481)
                    || (Math.Min(Encounter.BossId, other.Encounter.BossId) == 19651 && Math.Max(Encounter.BossId, other.Encounter.BossId) == 19844))
                {
                    var group = Group;
                    group.ExceptWith(other.Group);
                    return !group.Any();
                }
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Encounter.BossId.GetHashCode();
    }
}