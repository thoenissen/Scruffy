using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Guild user configurations
/// </summary>
[Table("GuildUserConfigurations")]
public class GuildUserConfigurationEntity
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
    /// Fixed guild rank
    /// </summary>
    public bool IsFixedRank { get; set; }

    /// <summary>
    /// Inactive user
    /// </summary>
    public bool IsInactive { get; set; }

    #region Navigation properties

    /// <summary>
    /// Guild
    /// </summary>
    [ForeignKey(nameof(GuildId))]
    public  virtual GuildEntity Guild { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}