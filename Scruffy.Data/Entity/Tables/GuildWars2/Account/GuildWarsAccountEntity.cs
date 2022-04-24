using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Enumerations.GuildWars2;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account;

/// <summary>
/// Account
/// </summary>
[Table("GuildWarsAccounts")]
public class GuildWarsAccountEntity
{
    #region Properties

    /// <summary>
    /// Account name
    /// </summary>
    [Key]
    [StringLength(42)]
    public string Name { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Api key
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Last age in seconds
    /// </summary>
    public long LastAge { get; set; }

    /// <summary>
    /// Id of the word
    /// </summary>
    public long? WorldId { get; set; }

    /// <summary>
    /// Daily achievement points
    /// </summary>
    public int? DailyAchievementPoints { get; set;  }

    /// <summary>
    /// Monthly achievements points
    /// </summary>
    public int? MonthlyAchievementPoints { get; set; }

    /// <summary>
    /// Permissions
    /// </summary>
    public GuildWars2ApiPermission Permissions { get; set; }

    #region Navigation properties

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}