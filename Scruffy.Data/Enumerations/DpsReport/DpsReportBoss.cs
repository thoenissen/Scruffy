using GW2EIEvtcParser;

namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// Boss entry for encounter
/// </summary>
public class DpsReportBoss
{
    /// <summary>
    /// Boss ID
    /// </summary>
    public SpeciesIDs.TargetID[] BossIds { get; set; }

    /// <summary>
    /// Boss name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Boss icon URL
    /// </summary>
    public string IconUrl { get; set; }

    /// <summary>
    /// Indicates if the boss has been successfully defeated
    /// </summary>
    public bool? IsSuccessful { get; set; }

    /// <summary>
    /// List of DPS logs for this boss
    /// </summary>
    public List<DpsReportBossLogEntry> Logs { get; set; } = [];

    /// <summary>
    /// Indicates whether the boss details (logs) are currently expanded
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Indicates whether logs are currently being loaded for this boss
    /// </summary>
    public bool IsLoadingLogs { get; set; }
}