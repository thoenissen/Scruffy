using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Discord;

/// <summary>
/// Historic discord role assignments
/// </summary>
[Table("DiscordHistoryRoleAssignments")]
public class DiscordHistoryRoleAssignmentEntity
{
    #region Properties

    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Id of the server
    /// </summary>
    public ulong ServerId  { get; set; }

    /// <summary>
    /// Id of the user
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// Id of the role
    /// </summary>
    public ulong RoleId { get; set; }

    #endregion // Properties
}