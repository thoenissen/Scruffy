namespace Scruffy.WebApp.DTOs.Calendar;

/// <summary>
/// Member connected to a voice channel
/// </summary>
public class VoiceChannelMemberDTO
{
    #region Properties

    /// <summary>
    /// Discord account ID
    /// </summary>
    public ulong DiscordAccountId { get; init; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; init; }

    #endregion // Properties
}