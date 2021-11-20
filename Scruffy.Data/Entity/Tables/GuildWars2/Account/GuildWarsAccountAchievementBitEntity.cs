using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account;

/// <summary>
/// Bits of the account achievement
/// </summary>
[Table("GuildWarsAccountAchievementBits")]
public class GuildWarsAccountAchievementBitEntity
{
    #region Properties

    /// <summary>
    /// Account name
    /// </summary>
    [StringLength(42)]
    public string AccountName { get; set; }

    /// <summary>
    /// Id of the achievement
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// Bit
    /// </summary>
    public int Bit { get; set; }

    #region Navigation properties

    /// <summary>
    /// Account
    /// </summary>
    [ForeignKey(nameof(AccountName))]
    public virtual GuildWarsAccountEntity GuildWarsAccount { get; set; }

    /// <summary>
    /// Account
    /// </summary>
    public virtual GuildWarsAccountAchievementEntity GuildWarsAccountAchievement { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}