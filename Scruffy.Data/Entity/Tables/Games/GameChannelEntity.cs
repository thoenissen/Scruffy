using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Enumerations.Games;

namespace Scruffy.Data.Entity.Tables.Games;

/// <summary>
/// Game channel
/// </summary>
[Table("GameChannels")]
public class GameChannelEntity
{
    #region Properties

    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public GameType Type { get; set; }

    /// <summary>
    /// Channel id
    /// </summary>
    public ulong DiscordChannelId { get; set; }

    #endregion // Properties
}