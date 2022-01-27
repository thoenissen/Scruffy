namespace Scruffy.Services.Core.Extensions;

/// <summary>
/// <see cref="DiscordMember"/>-Extensions
/// </summary>
public static class DiscordMemberExtension
{
    /// <summary>
    /// Gets the best display name
    /// </summary>
    /// <param name="member">Member</param>
    /// <returns>Display name</returns>
    public static string TryGetDisplayName(this DiscordMember member) => string.IsNullOrWhiteSpace(member.Nickname)
                                                                             ? string.IsNullOrWhiteSpace(member.DisplayName)
                                                                                   ? member.Username
                                                                                   : member.DisplayName
                                                                             : member.Nickname;
}