using GW2EIEvtcParser;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Boss entry for encounter
/// </summary>
public class DpsReportBoss
{
    /// <summary>
    /// Gets or sets the boss ID
    /// </summary>
    public SpeciesIDs.TargetID BossId { get; set; }

    /// <summary>
    /// Gets or sets the boss name
    /// </summary>
    public string Name { get; set; }
}