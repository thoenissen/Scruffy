using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Assigning roles to ignore
/// </summary>
[Table("GuildSpecialRankIgnoreRoleAssignments")]
public class GuildSpecialRankIgnoreRoleAssignmentEntity
{
    #region Properties

    /// <summary>
    /// Id of the configuration
    /// </summary>
    public long ConfigurationId { get; set; }

    /// <summary>
    /// Id of the discord role
    /// </summary>
    public ulong DiscordRoleId { get; set; }

    #region Navigation properties

    /// <summary>
    /// Configuration
    /// </summary>
    [ForeignKey(nameof(ConfigurationId))]
    public virtual GuildSpecialRankConfigurationEntity GuildSpecialRankConfiguration { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}