using Scruffy.Data.Json.DpsReport;

namespace Scruffy.Data.Services.DpsReport;

/// <summary>
/// Stats for a DPS report player group
/// </summary>
public class PlayerGroupStats
{
    /// <summary>
    /// Total encounter time of all encounters
    /// </summary>
    public TimeSpan FailedEncounterTotalTime { get; private set; }

    /// <summary>
    /// Total encounter time of all encounters
    /// </summary>
    public TimeSpan SuccessfulEncounterTotalTime { get; private set; }

    /// <summary>
    /// Time of the first encounter
    /// </summary>
    public DateTimeOffset FirstEncounterTime { get; private set; }

    /// <summary>
    /// Time of the last encounter
    /// </summary>
    public DateTimeOffset LastEncounterTime { get; private set; }

    /// <summary>
    /// Total amount of succesful encounters
    /// </summary>
    public int SuccessfulEncounters { get; private set; }

    /// <summary>
    /// Average DPS across all encounters
    /// </summary>
    public long AverageDPS => DPSEncounter > 0 ? CumulatedDPS / DPSEncounter : 0;

    #region PrivateFields

    /// <summary>
    /// Cumulated computed DPS
    /// </summary>
    private long CumulatedDPS { get; set; }

    /// <summary>
    /// How many encounter had a valid DPS value
    /// </summary>
    private int DPSEncounter { get; set; }

    #endregion // PrivateFields

    /// <summary>
    /// Constructor
    /// </summary>
    public PlayerGroupStats()
    {
        FirstEncounterTime = DateTimeOffset.MaxValue;
        LastEncounterTime = DateTimeOffset.MinValue;
    }

    /// <summary>
    /// Adds an encounter to the group stats
    /// </summary>
    /// <param name="upload">Upload of the encounter</param>
    public void AddEncounter(Upload upload)
    {
        var startTime = upload.EncounterTime;
        if (startTime < FirstEncounterTime)
        {
            FirstEncounterTime = startTime;
        }

        var endTime = startTime + upload.Encounter.Duration;
        if (endTime > LastEncounterTime)
        {
            LastEncounterTime = endTime;
        }

        if (upload.Encounter.CompDps > 0)
        {
            CumulatedDPS += upload.Encounter.CompDps;
            ++DPSEncounter;
        }

        if (upload.Encounter.Success)
        {
            SuccessfulEncounterTotalTime += upload.Encounter.Duration;
            ++SuccessfulEncounters;
        }
        else
        {
            FailedEncounterTotalTime += upload.Encounter.Duration;
        }
    }
}