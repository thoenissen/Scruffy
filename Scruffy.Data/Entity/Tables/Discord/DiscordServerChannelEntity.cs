using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Discord;

/// <summary>
/// Discord server channel
/// </summary>
[Table("DiscordServerChannels")]
public class DiscordServerChannelEntity
{
    #region Properties

    /// <summary>
    /// Discord server id
    /// </summary>
    public ulong ServerId { get; set; }

    /// <summary>
    /// Discord channel id
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// Channel name
    /// </summary>
    public string Name { get; set; }

    #endregion // Properties
}