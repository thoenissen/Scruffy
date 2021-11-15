using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.CoreData;

/// <summary>
/// Configuration of a server
/// </summary>
[Table("ServerConfigurations")]
public class ServerConfigurationEntity
{
    #region Properties

    /// <summary>
    /// Id of the server
    /// </summary>
    [Key]
    public ulong DiscordServerId { get; set; }

    /// <summary>
    /// Prefix
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// Administration role
    /// </summary>
    public ulong? DiscordAdministratorRoleId { get; set; }

    #endregion // Properties
}