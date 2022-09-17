using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Raid;

/// <summary>
/// Raid special role
/// </summary>
[Table("RaidSpecialRoles")]
public class RaidSpecialRoleEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Emoji id
    /// </summary>
    public ulong DiscordEmojiId { get; set; }

    #endregion // Properties
}