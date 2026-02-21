namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// Expansion entry
/// </summary>
public class DpsReportExpansionEntry
{
    #region Properties

    /// <summary>
    /// Expansion ID
    /// </summary>
    public DpsReportExpansion ExpansionId { get; set; }

    /// <summary>
    /// Expansion name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Icon URL
    /// </summary>
    public string IconUrl { get; set; }

    /// <summary>
    /// List of encounters in this expansion
    /// </summary>
    public List<DpsReportEncounterEntry> Encounters { get; set; } = [];

    #endregion // Properties
}