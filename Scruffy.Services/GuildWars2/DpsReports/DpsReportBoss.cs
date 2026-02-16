using GW2EIEvtcParser;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Boss entry for encounter
/// </summary>
public class DpsReportBoss
{
    /// <summary>
    /// Boss ID
    /// </summary>
    public SpeciesIDs.TargetID BossId { get; set; }

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
}