using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Upload
/// </summary>
public class Upload
{
    #region Fields

    /// <summary>
    /// Fight name
    /// </summary>
    private string _fightName;

    #endregion // Fields

    #region Properties

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

    /// <summary>
    /// Language
    /// </summary>
    [JsonProperty("language")]
    public string Language { get; set; }

    /// <summary>
    /// Language id
    /// </summary>
    [JsonProperty("languageId")]
    public int? LanguageId { get; set; }

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
    /// Fight name
    /// </summary>
    public string FightName
    {
        get => _fightName ?? Encounter?.Boss;
        set => _fightName = value;
    }

    /// <summary>
    /// HashSet of all player's unique names
    /// </summary>
    public HashSet<string> Group => Players.Select(obj => obj.Value.DisplayName).Distinct().ToHashSet();

    #endregion // Properties

    #region Json

    /// <summary>
    /// JSON error handles
    /// </summary>
    /// <param name="context">Streaming context</param>
    /// <param name="errorContext">Error context</param>
    [OnError]
#pragma warning disable IDE0060
    internal void OnError(StreamingContext context, ErrorContext errorContext)
#pragma warning restore IDE0060
    {
        // If no players exist the players are represented as empty array and so the serialization into a dictionary fails
        if (errorContext.Member?.Equals("players") == true)
        {
            errorContext.Handled = true;
        }
    }

    #endregion // Json

    #region Object

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

    #endregion // Object
}