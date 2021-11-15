using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account;

/// <summary>
/// Daily logins
/// </summary>
[Table("GuildWarsAccountDailyLoginChecks")]
public class GuildWarsAccountDailyLoginCheckEntity
{
    #region Properties

    /// <summary>
    /// Account name
    /// </summary>
    [StringLength(42)]
    public string Name { get; set; }

    /// <summary>
    /// Day
    /// </summary>
    public DateTime Date { get; set; }

    #region Navigation properties

    /// <summary>
    /// Account
    /// </summary>
    [ForeignKey(nameof(Name))]
    public GuildWarsAccountEntity Account { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}