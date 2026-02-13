using System;
using System.Collections.Generic;

using GW2EIJSON;

namespace Scruffy.WebApp.Components.Pages.DpsReports.Data;

/// <summary>
/// Represents a single DPS report entry for a raid or encounter.
/// </summary>
public class DpsReport
{
    /// <summary>
    /// Meta data
    /// </summary>
    public MetaData MetaData { get; set; }

    /// <summary>
    /// Indicates whether additional data is currently being loaded for this report
    /// </summary>
    public bool IsLoadingAdditionalData { get; set; }

    /// <summary>
    /// Full report
    /// </summary>
    public JsonLog FullReport { get; set; }

    /// <summary>
    /// Overall statistics
    /// </summary>
    public OverallStatistics OverallStatistics { get; set; }

    /// <summary>
    /// Personal statistics
    /// </summary>
    public PersonalStatistics PersonalStatistics { get; set; }
}