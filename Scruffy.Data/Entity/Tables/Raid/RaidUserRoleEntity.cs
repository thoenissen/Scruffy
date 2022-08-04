using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Role of an user
/// </summary>
[Table("RaidUserRoles")]
public class RaidUserRoleEntity
{
    #region Properties

    /// <summary>
    /// Id of the user
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Id of the role
    /// </summary>
    public long RoleId { get; set; }

    #region Navigation properties

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; }

    /// <summary>
    /// Main role
    /// </summary>
    [ForeignKey(nameof(RoleId))]
    public virtual RaidRoleEntity RaidRole { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}