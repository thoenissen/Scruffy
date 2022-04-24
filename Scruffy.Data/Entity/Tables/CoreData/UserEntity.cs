using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Enumerations.CoreData;

namespace Scruffy.Data.Entity.Tables.CoreData;

/// <summary>
/// Discord user
/// </summary>
[Table("Users")]
public class UserEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Creation of the user
    /// </summary>
    public DateTime CreationTimeStamp { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public UserType Type { get; set; }

    /// <summary>
    /// Experience level
    /// </summary>
    public long? RaidExperienceLevelId { get; set; }

    /// <summary>
    /// GitHub-Account
    /// </summary>
    public string GitHubAccount { get; set; }

    /// <summary>
    /// dps.report user token
    /// </summary>
    public string DpsReportUserToken { get; set; }

    /// <summary>
    /// Are the data storage terms accepted?
    /// </summary>
    public bool? IsDataStorageAccepted { get; set; }

    /// <summary>
    /// Are the extended data storage terms accepted?
    /// </summary>
    public bool? IsExtendedDataStorageAccepted { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Birthday
    /// </summary>
    public DateTime? Birthday { get; set; }

    #region Navigation properties

    /// <summary>
    /// Raid experience level
    /// </summary>
    public virtual RaidExperienceLevelEntity RaidExperienceLevel { get; set; }

    /// <summary>
    /// Raid registrations
    /// </summary>
    public virtual ICollection<RaidRegistrationEntity> RaidRegistrations { get; set; }

    /// <summary>
    /// Raid roles
    /// </summary>
    public virtual ICollection<RaidUserRoleEntity> RaidUserRoles { get; set; }

    /// <summary>
    /// Accounts
    /// </summary>
    public virtual ICollection<GuildWarsAccountEntity> GuildWarsAccounts { get; set; }

    /// <summary>
    /// Discord user
    /// </summary>
    public virtual ICollection<DiscordAccountEntity> DiscordAccounts { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}