using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Raid role
/// </summary>
[Table("RaidRoles")]
public class RaidRoleEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Discord emoji
    /// </summary>
    public ulong DiscordEmojiId { get; set; }

    /// <summary>
    /// Tank
    /// </summary>
    public bool IsTank { get; set; }

    /// <summary>
    /// Alacrity
    /// </summary>
    public bool IsProvidingAlacrity { get; set; }

    /// <summary>
    /// Quickness
    /// </summary>
    public bool IsProvidingQuickness { get; set; }

    /// <summary>
    /// Healer
    /// </summary>
    public bool IsHealer { get; set; }

    /// <summary>
    /// Damage dealer
    /// </summary>
    public bool IsDamageDealer { get; set; }

    #endregion // Properties
}