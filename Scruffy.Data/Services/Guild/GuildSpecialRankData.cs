namespace Scruffy.Data.Services.Guild;

/// <summary>
/// Special rank
/// </summary>
public class GuildSpecialRankData
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Id of the discord role
    /// </summary>
    public ulong DiscordRoleId { get; set; }
}