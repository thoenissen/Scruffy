namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// Represents a single DPS log entry for a boss
/// </summary>
public class DpsReportBossLogEntry
{
    /// <summary>
    /// Log ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Permanent link to the log
    /// </summary>
    public string PermaLink { get; set; }

    /// <summary>
    /// Encounter time
    /// </summary>
    public DateTime EncounterTime { get; set; }

    /// <summary>
    /// Indicates whether the encounter was successful
    /// </summary>
    public bool IsSuccess { get; set; }
}