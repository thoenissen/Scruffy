using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Encounter entry for expansion
/// </summary>
public class DpsReportEncounterEntry
{
    #region Properties

    /// <summary>
    /// Gets or sets the encounter ID
    /// </summary>
    public DpsReportEncounter EncounterId { get; set; }

    /// <summary>
    /// Gets or sets the encounter name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the encounter icon URL
    /// </summary>
    public string IconUrl { get; set; }

    /// <summary>
    /// Gets or sets the list of bosses in this encounter
    /// </summary>
    public List<DpsReportBoss> Bosses { get; set; } = [];

    #endregion // Properties
}