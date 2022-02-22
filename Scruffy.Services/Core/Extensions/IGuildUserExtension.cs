using Discord;

namespace Scruffy.Services.Core.Extensions;

/// <summary>
/// <see cref="IGuildUser"/>-Extensions
/// </summary>
public static class IGuildUserExtension
{
    /// <summary>
    /// Gets the best display name
    /// </summary>
    /// <param name="member">Member</param>
    /// <returns>Display name</returns>
    public static string TryGetDisplayName(this IGuildUser member) => string.IsNullOrWhiteSpace(member?.Nickname)
                                                                             ? member?.Username
                                                                             : member.Nickname;
}