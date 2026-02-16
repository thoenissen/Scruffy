using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Encounter entry for expansion
/// </summary>
public class DpsReportEncounterEntry
{
    #region Properties

    /// <summary>
    /// Encounter ID
    /// </summary>
    public DpsReportEncounter EncounterId { get; set; }

    /// <summary>
    /// Encounter name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Bosses in this encounter
    /// </summary>
    public List<DpsReportBoss> Bosses { get; set; } = [];

    #endregion // Properties
}