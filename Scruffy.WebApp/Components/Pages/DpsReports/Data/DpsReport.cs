using System;
using System.Collections.Generic;

namespace Scruffy.WebApp.Components.Pages.DpsReports.Data;

/// <summary>
/// Represents a single DPS report entry for a raid or encounter.
/// </summary>
public class DpsReport
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
    /// Indicates whether additional data is currently being loaded for this report
    /// </summary>
    public bool IsLoadingAdditionalData { get; set; }

    /// <summary>
    /// Additional parsed data from the DPS report.
    /// </summary>
    public AdditionalData AdditionalData { get; set; }

    /// <summary>
    /// Duration of the encounter.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Character name of the own player in this encounter
    /// </summary>
    public string PlayerCharacterName { get; set; }

    /// <summary>
    /// Mechanics data for the own player
    /// </summary>
    public List<Mechanic> Mechanics { get; set; } = [];
}