using System;
using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Tables.Statistics;

/// <summary>
/// Discord voice statistics data
/// </summary>
[Table("DiscordVoiceTimeSpans")]
public class DiscordVoiceTimeSpanEntity
{
    #region Properties

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
    /// Start of the session
    /// </summary>
    public DateTime StartTimeStamp { get; set; }

    /// <summary>
    /// Ending of the session
    /// </summary>
    public DateTime EndTimeStamp { get; set; }

    /// <summary>
    /// Is the voice session completed or ongoing?
    /// </summary>
    public bool IsCompleted { get; set; }

    #region Navigation - Properties

    /// <summary>
    /// Discord account
    /// </summary>
    [ForeignKey(nameof(DiscordAccountId))]
    public DiscordAccountEntity DiscordAccount { get; set; }

    #endregion // Navigation - Properties

    #endregion // Properties
}