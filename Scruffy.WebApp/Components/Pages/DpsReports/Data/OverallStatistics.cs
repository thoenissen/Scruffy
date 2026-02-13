namespace Scruffy.WebApp.Components.Pages.DpsReports.Data;

/// <summary>
/// Overall statistics
/// </summary>
public class OverallStatistics
{
    /// <summary>
    /// The DPS (damage per second) value for the encounter.
    /// </summary>
    public int? Dps { get; set; }

    /// <summary>
    /// The average alacrity uptime percentage during the encounter.
    /// </summary>
    public double? Alacrity { get; set; }

    /// <summary>
    /// The average quickness uptime percentage during the encounter.
    /// </summary>
    public double? Quickness { get; set; }
}