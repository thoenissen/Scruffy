using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace Scruffy.Data.Entity.Tables.Discord;

/// <summary>
/// Chat experience log
/// </summary>
[Table("DiscordMessageExperienceLogEntries")]
[PrimaryKey(nameof(ServerId), nameof(MessageId))]
public class DiscordMessageExperienceLogEntryEntity
{
    #region Properties

    /// <summary>
    /// Server ID
    /// </summary>
    public ulong ServerId { get; set; }

    /// <summary>
    /// Message ID
    /// </summary>
    public ulong MessageId { get; set; }

    /// <summary>
    /// Discord account ID
    /// </summary>
    public ulong DiscordAccountId { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Experience points
    /// </summary>
    public ushort ExperiencePoints { get; set; }

    #endregion // Properties
}