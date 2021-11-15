namespace Scruffy.Data.Services.Statistics;

/// <summary>
/// Message data
/// </summary>
public class DiscordMessageBulkInsertData
{
    /// <summary>
    /// Id of the server
    /// </summary>
    public ulong ServerId { get; set; }

    /// <summary>
    /// Id of the channel
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// Id of the message
    /// </summary>
    public ulong MessageId { get; set; }

    /// <summary>
    /// Time stamp
    /// </summary>
    public DateTime TimeStamp { get; set; }
}