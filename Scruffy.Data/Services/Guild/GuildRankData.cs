namespace Scruffy.Data.Services.Guild;

/// <summary>
/// Rank
/// </summary>
public class GuildRankData
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// In game name
    /// </summary>
    public string InGameName { get; set; }

    /// <summary>
    /// Id of the discord role
    /// </summary>
    public ulong DiscordRoleId { get; set; }
}