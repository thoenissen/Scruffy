using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Discord;

/// <summary>
/// Discord server member
/// </summary>
[Table("DiscordServerMembers")]
public class DiscordServerMemberEntity
{
    #region Properties

    /// <summary>
    /// Discord server id
    /// </summary>
    public ulong ServerId { get; set; }

    /// <summary>
    /// Discord account id
    /// </summary>
    public ulong AccountId { get; set; }

    /// <summary>
    /// Server specific user name
    /// </summary>
    public string Name { get; set; }

    #region Navigation properties

    /// <summary>
    /// Account
    /// </summary>
    [ForeignKey(nameof(AccountId))]
    public DiscordAccountEntity Account { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}