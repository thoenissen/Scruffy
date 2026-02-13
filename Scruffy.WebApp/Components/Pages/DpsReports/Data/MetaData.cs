using System;

namespace Scruffy.WebApp.Components.Pages.DpsReports.Data;

/// <summary>
/// Meta data
/// </summary>
public class MetaData
{
    /// <summary>
    /// Unique identifier of the report.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Indicates whether the encounter was a success.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Permanent link to the uploaded DPS report.
    /// </summary>
    public string PermaLink { get; set; }

    /// <summary>
    /// Timestamp of when the encounter took place.
    /// </summary>
    public DateTimeOffset EncounterTime { get; set; }

    /// <summary>
    /// Name of the boss for this encounter.
    /// </summary>
    public string Boss { get; set; }

    /// <summary>
    /// Duration of the encounter.
    /// </summary>
    public TimeSpan? Duration { get; set; }
}