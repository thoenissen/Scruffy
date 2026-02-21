namespace Scruffy.Data.Services.DpsReport;

/// <summary>
/// Encounter data
/// </summary>
public class DpsReportEncounterData
{
    /// <summary>
    /// Permalink
    /// </summary>
    public string PermaLink { get; set; }

    /// <summary>
    /// Encounter time
    /// </summary>
    public DateTime EncounterTime { get; set; }

    /// <summary>
    /// Is the encounter successful?
    /// </summary>
    public bool IsSuccess { get; set; }
}