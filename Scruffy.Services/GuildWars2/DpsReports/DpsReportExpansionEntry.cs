using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Services.GuildWars2.DpsReports;

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
    /// List of encounters in this expansion
    /// </summary>
    public List<DpsReportEncounterEntry> Encounters { get; set; } = [];

    #endregion // Properties
}