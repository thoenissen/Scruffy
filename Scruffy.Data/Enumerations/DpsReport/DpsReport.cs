using GW2EIJSON;

using Scruffy.Data.Services.DpsReport;

namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// Represents a single DPS report entry for a raid or encounter.
/// </summary>
public class DpsReport
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public DpsReport()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logEntry">Log entry</param>
    public DpsReport(DpsReportBossLogEntry logEntry)
    {
        MetaData = new MetaData
                   {
                       Id = logEntry.Id,
                       IsSuccess = logEntry.IsSuccess,
                       PermaLink = logEntry.PermaLink,
                       EncounterTime = new DateTimeOffset(logEntry.EncounterTime),
                       Boss = "Loading...",
                       Duration = null
                   };
        IsLoadingAdditionalData = true;
    }

    #endregion // Constructor

    #region Properties

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

    #endregion // Properties
}