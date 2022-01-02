using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Guild;

/// <summary>
/// Guild ranks
/// </summary>
[Table("GuildRanks")]
public class GuildRankEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Id of the guild
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Ordering the ranks
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Discord role id
    /// </summary>
    public ulong DiscordRoleId { get; set; }

    /// <summary>
    /// In-game rank
    /// </summary>
    public string InGameName { get; set; }

    /// <summary>
    /// Percentage
    /// </summary>
    public double Percentage { get; set; }

    #region Navigation properties

    /// <summary>
    /// Guild
    /// </summary>
    [ForeignKey(nameof(GuildId))]
    public GuildEntity Guild { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}