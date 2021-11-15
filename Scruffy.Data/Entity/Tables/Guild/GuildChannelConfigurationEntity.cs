using System.ComponentModel.DataAnnotations.Schema;
using Scruffy.Data.Enumerations.GuildAdministration;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Guild notification channel
/// </summary>
[Table("GuildChannelConfigurations")]
public class GuildChannelConfigurationEntity
{
    #region Properties

    /// <summary>
    /// Id of the guild
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public GuildChannelConfigurationType Type { get; set; }

    /// <summary>
    /// Id of the channel
    /// </summary>
    public ulong DiscordChannelId { get; set; }

    /// <summary>
    /// Id of the message
    /// </summary>
    public ulong? DiscordMessageId { get; set; }

    /// <summary>
    /// Additional data
    /// </summary>
    public string AdditionalData { get; set; }

    #region Navigation properties

    /// <summary>
    /// Guild
    /// </summary>
    public GuildEntity Guild { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}