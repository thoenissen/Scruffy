namespace Scruffy.WebApp.Components.Pages.DpsReports.Data;

/// <summary>
/// Represents a mechanic from a DPS report with hit count.
/// </summary>
public class Mechanic
{
    /// <summary>
    /// Short name of the mechanic
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Full display name of the mechanic
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Description of the mechanic
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Number of times the player was hit by this mechanic
    /// </summary>
    public int Count { get; set; }
}