using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account;

/// <summary>
/// Guild Wars account guild ranking data
/// </summary>
[Table("GuildWarsAccountRankingData")]
public class GuildWarsAccountRankingDataEntity
{
    #region Properties

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
    /// Achievement Points
    /// </summary>
    public long? AchievementPoints { get; set; }

    #endregion // Properties
}