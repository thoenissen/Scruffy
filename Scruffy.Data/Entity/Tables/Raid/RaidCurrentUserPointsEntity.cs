using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Current raid points of a user
/// </summary>
[Table("RaidCurrentUserPoints")]
public class RaidCurrentUserPointsEntity
{
    #region Properties

    /// <summary>
    /// Id of the user
    /// </summary>
    [Key]
    public long UserId { get; set; }

    /// <summary>
    /// Current points
    /// </summary>
    public double Points { get; set; }

    #region Navigation properties

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}