using System.Runtime.Serialization;

using GW2EIEvtcParser;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Scruffy.Data.Json.DpsReport;

/// <summary>
/// Upload
/// </summary>
public class Upload
{
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
    public Dictionary<string, Player> Players { get; set; } = [];

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
        get => field ?? Encounter?.Boss;
        set;
    }

    /// <summary>
    /// HashSet of all player's unique names
    /// </summary>
    public HashSet<string> Group => Players?.Select(obj => obj.Value.DisplayName).Distinct().ToHashSet() ?? [];

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Check if two boss IDS belong to the same encounter
    /// </summary>
    /// <param name="leftBossId">Left boss ID</param>
    /// <param name="rightBossId">Right boss ID</param>
    /// <returns>Do they belong to the same encounter?</returns>
    private static bool IsSameEncounter(int leftBossId, int rightBossId)
    {
        var leftTargetId = SpeciesIDs.GetTargetID(leftBossId);

        switch (leftTargetId)
        {
            case SpeciesIDs.TargetID.VoiceOfTheFallen:
                leftTargetId = SpeciesIDs.TargetID.ClawOfTheFallen;
                break;

            case SpeciesIDs.TargetID.EyeOfJudgement:
                leftTargetId = SpeciesIDs.TargetID.EyeOfFate;
                break;
        }

        var rightTargetId = SpeciesIDs.GetTargetID(rightBossId);

        switch (rightTargetId)
        {
            case SpeciesIDs.TargetID.VoiceOfTheFallen:
                rightTargetId = SpeciesIDs.TargetID.ClawOfTheFallen;
                break;

            case SpeciesIDs.TargetID.EyeOfJudgement:
                rightTargetId = SpeciesIDs.TargetID.EyeOfFate;
                break;
        }

        return leftTargetId == rightTargetId;
    }

    #endregion // Methods

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
            Players = [];

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
            if (other.Id == Id)
            {
                return true;
            }

            if (Math.Abs((EncounterTime - other.EncounterTime).TotalSeconds) < 5.0)
            {
                if (IsSameEncounter(Encounter.BossId, other.Encounter.BossId))
                {
                    var group = Group;

                    group.ExceptWith(other.Group);

                    return group.Count == 0;
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