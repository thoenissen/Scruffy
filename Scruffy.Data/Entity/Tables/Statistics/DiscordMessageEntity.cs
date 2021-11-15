using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Statistics;

/// <summary>
/// Discord message statistics data
/// </summary>
[Table("DiscordMessages")]
public class DiscordMessageEntity
{
    /// <summary>
    /// Id of the server
    /// </summary>
    public ulong DiscordServerId { get; set; }

    /// <summary>
    /// Id of the channel
    /// </summary>
    public ulong DiscordChannelId { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public ulong DiscordAccountId { get; set; }

    /// <summary>
    /// Id of the message
    /// </summary>
    public ulong DiscordMessageId { get; set; }

    /// <summary>
    /// Time stamp
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Is the entry committed by the batch import?
    /// </summary>
    public bool IsBatchCommitted { get; set; }
}