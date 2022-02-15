using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Discord;

/// <summary>
/// Discord role assignment history
/// </summary>
[Table("DiscordHistoricAccountRoleAssignments")]
public class DiscordHistoricAccountRoleAssignmentEntity
{
    #region Properties

    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Id of the server
    /// </summary>
    public ulong ServerId { get; set; }

    /// <summary>
    /// Id of the role
    /// </summary>
    public ulong RoleId  { get; set; }

    /// <summary>
    /// Id of the account
    /// </summary>
    public ulong AccountId { get; set; }

    #endregion // Properties
}