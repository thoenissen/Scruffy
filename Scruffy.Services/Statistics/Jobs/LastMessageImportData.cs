namespace Scruffy.Services.Statistics.Jobs;

/// <summary>
/// Last import
/// </summary>
public class LastMessageImportData
{
    /// <summary>
    /// Server ID
    /// </summary>
    public ulong ServerId { get; set; }

    /// <summary>
    /// Channel ID
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// Thread ID
    /// </summary>
    public ulong ThreadId { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime TimeStamp { get; set; }
}