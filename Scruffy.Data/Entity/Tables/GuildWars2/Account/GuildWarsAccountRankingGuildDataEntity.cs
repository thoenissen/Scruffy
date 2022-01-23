using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account;

/// <summary>
/// Guild Wars account guild ranking guild data
/// </summary>
[Table("GuildWarsAccountRankingGuildData")]
public class GuildWarsAccountRankingGuildDataEntity
{
    #region Properties

    /// <summary>
    /// Id of the guild
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Account name
    /// </summary>
    [StringLength(42)]
    public string AccountName { get; set; }

    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Representation percentage
    /// </summary>
    public int? RepresentationPercentage { get; set; }

    /// <summary>
    /// Donation value
    /// </summary>
    public ulong? DonationValue { get; set; }

    #endregion // Properties
}