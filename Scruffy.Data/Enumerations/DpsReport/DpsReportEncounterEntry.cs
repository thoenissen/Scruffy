namespace Scruffy.Data.Enumerations.DpsReport;

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
    /// Encounter icon URL
    /// </summary>
    public string IconUrl { get; set; }

    /// <summary>
    /// List of bosses in this encounter
    /// </summary>
    public List<DpsReportBoss> Bosses { get; set; } = [];

    #endregion // Properties
}