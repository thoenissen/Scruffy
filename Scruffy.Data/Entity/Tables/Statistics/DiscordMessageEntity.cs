using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Statistics;

/// <summary>
/// Discord message statistics data
/// </summary>
[Table("DiscordMessages")]
public class DiscordMessageEntity
{
    /// <summary>
    /// ID of the server
    /// </summary>
    public ulong DiscordServerId { get; set; }

    /// <summary>
    /// ID of the channel
    /// </summary>
    public ulong DiscordChannelId { get; set; }

    /// <summary>
    /// ID of the thread
    /// </summary>
    public ulong DiscordThreadId { get; set; }

    /// <summary>
    /// ID of the user
    /// </summary>
    public ulong DiscordAccountId { get; set; }

    /// <summary>
    /// ID of the message
    /// </summary>
    public ulong DiscordMessageId { get; set; }

    /// <summary>
    /// Time stamp
    /// </summary>
    public DateTime TimeStamp { get; set; }
}