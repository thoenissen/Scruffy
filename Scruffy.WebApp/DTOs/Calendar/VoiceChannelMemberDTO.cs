namespace Scruffy.WebApp.DTOs.Calendar;

/// <summary>
/// Member connected to a voice channel
/// </summary>
public class VoiceChannelMemberDTO
{
    /// <summary>
    /// Discord account ID
    /// </summary>
    public ulong DiscordAccountId { get; init; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; init; }
}