using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Guild donations
/// </summary>
[Table("GuildDonations")]
public class GuildDonationEntity
{
    #region Properties

    /// <summary>
    /// Id of the guild
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Id of the log entry
    /// </summary>
    public int LogEntryId { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public long Value { get; set; }

    /// <summary>
    /// Is this relevant for the donation threshold?
    /// </summary>
    public bool IsThresholdRelevant { get; set; }

    #endregion // Properties
}