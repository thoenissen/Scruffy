using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Enumerations.Guild;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Guild activity rank notification
/// </summary>
[Table("GuildRankNotifications")]
public class GuildRankNotificationEntity
{
    #region Properties

    /// <summary>
    /// Id of the guild
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Typ
    /// </summary>
    public DiscordActivityPointsType Type { get; set; }

    /// <summary>
    /// Id of the discord role
    /// </summary>
    public ulong RoleId { get; set; }

    #region Methods

    /// <summary>
    /// Guild
    /// </summary>
    [ForeignKey(nameof(GuildId))]
    public virtual GuildEntity Guild { get; set; }

    /// <summary>
    /// User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; }

    #endregion // Methods

    #endregion // Properties
}