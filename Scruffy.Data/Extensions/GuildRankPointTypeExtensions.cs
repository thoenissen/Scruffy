using Scruffy.Data.Enumerations.Guild;

namespace Scruffy.Data.Extensions;

/// <summary>
/// Extensions for <see cref="GuildRankPointType"/>
/// </summary>
public static class GuildRankPointTypeExtensions
{
    /// <summary>
    /// Get color
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>Color</returns>
    public static string GetColor(this GuildRankPointType type)
    {
        return type switch
               {
                   GuildRankPointType.Login => "#1E6FA3",
                   GuildRankPointType.Representation => "#CC4F6A",
                   GuildRankPointType.AchievementPoints => "#CC9E44",
                   GuildRankPointType.Membership => "#2F7A7A",
                   GuildRankPointType.Donation => "#6B46C1",
                   GuildRankPointType.DiscordVoiceActivity => "#CC7F33",
                   GuildRankPointType.DiscordMessageActivity => "#8C8F93",
                   GuildRankPointType.Events => "#2B8FB5",
                   GuildRankPointType.Development => "#AD4A86",
                   _ => "#FFFFFF"
               };
    }
}