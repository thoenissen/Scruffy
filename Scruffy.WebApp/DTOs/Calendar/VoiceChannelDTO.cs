using System.Collections.Generic;

namespace Scruffy.WebApp.DTOs.Calendar;

/// <summary>
/// Voice channel with connected members
/// </summary>
public class VoiceChannelDTO
{
    /// <summary>
    /// Channel ID
    /// </summary>
    public ulong ChannelId { get; init; }

    /// <summary>
    /// Channel name
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Members currently connected to this voice channel
    /// </summary>
    public List<VoiceChannelMemberDTO> Members { get; init; } = [];
}