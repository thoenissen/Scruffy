using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account;

/// <summary>
/// Achievement of an account
/// </summary>
[Table("GuildWarsAccountAchievements")]
public class GuildWarsAccountAchievementEntity
{
    #region Properties

    /// <summary>
    /// Account name
    /// </summary>
    [StringLength(42)]
    public string AccountName { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// Current progress
    /// </summary>
    public int? Current { get; set; }

    /// <summary>
    /// Maximum progress
    /// </summary>
    public int? Maximum { get; set; }

    /// <summary>
    /// Is the achievement done?
    /// </summary>
    public bool IsDone { get; set; }

    /// <summary>
    /// Count of repetition
    /// </summary>
    public int? RepetitionCount { get; set; }

    /// <summary>
    /// Is the achievement unlocked?
    /// </summary>
    public bool IsUnlocked { get; set; }

    #region Navigation properties

    /// <summary>
    /// Account
    /// </summary>
    [ForeignKey(nameof(AccountName))]
    public virtual GuildWarsAccountEntity GuildWarsAccount { get; set; }

    /// <summary>
    /// Bits
    /// </summary>
    public virtual ICollection<GuildWarsAccountAchievementBitEntity> GuildWarsAccountAchievementBits { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}