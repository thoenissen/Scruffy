namespace Scruffy.Data.Services.Guild;

/// <summary>
/// Guild ranking overview data
/// </summary>
public class GuildRankingOverviewData
{
    #region Properties

    /// <summary>
    /// Creation time stamp
    /// </summary>
    public DateTime TimeStamp { get; } = DateTime.Now;

    /// <summary>
    /// User count
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// Pages
    /// </summary>
    public List<List<OverviewUserPointsData>> Pages { get; set; }

    #endregion // Properties
}