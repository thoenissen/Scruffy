using Scruffy.Data.Json.DpsReport;

namespace Scruffy.Data.Services.DpsReport;

/// <summary>
/// Stats for a DPS report player group
/// </summary>
public class PlayerGroupStats
{
    #region Fields

    /// <summary>
    /// Cumulated computed DPS
    /// </summary>
    private long _cumulatedDPS;

    /// <summary>
    /// How many encounter had a valid DPS value
    /// </summary>
    private int _dpsEncounter;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public PlayerGroupStats()
    {
        FirstEncounterTime = DateTimeOffset.MaxValue;
        LastEncounterTime = DateTimeOffset.MinValue;
    }

    #endregion // Constructor

    #region Properties

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
    public long AverageDPS => _dpsEncounter > 0 ? _cumulatedDPS / _dpsEncounter : 0;

    #endregion // Properties

    #region Methods

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
            _cumulatedDPS += upload.Encounter.CompDps;
            ++_dpsEncounter;
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

    #endregion // Methods
}