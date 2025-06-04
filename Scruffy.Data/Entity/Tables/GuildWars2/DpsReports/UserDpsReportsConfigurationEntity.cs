using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.GuildWars2.DpsReports;

/// <summary>
/// User dps.report configuration
/// </summary>
[Table("UserDpsReportsConfigurations")]
public class UserDpsReportsConfigurationEntity
{
    #region Properties

    /// <summary>
    /// User id
    /// </summary>
    [Key]
    public long UserId { get; set; }

    /// <summary>
    /// User token for dps.report
    /// </summary>
    [StringLength(64)]
    public string UserToken { get; set; }

    /// <summary>
    /// Last import date
    /// </summary>
    public DateTime? LastImport { get; set; }

    /// <summary>
    /// Is the import activated?
    /// </summary>
    public bool IsImportActivated { get; set; }

    #region Navigation properties

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}