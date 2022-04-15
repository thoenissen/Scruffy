namespace Scruffy.Data.Services.Guild;

/// <summary>
/// User point data
/// </summary>
public class OverviewUserPointsData
{
    /// <summary>
    /// User id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// User color
    /// </summary>
    public string UserColor { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    public double Points { get; set; }

    /// <summary>
    /// Discord user id
    /// </summary>
    public ulong? DiscordUserId { get; set; }

    /// <summary>
    /// Discord role id
    /// </summary>
    public ulong? DiscordRoleId { get; set; }
}