using Discord.WebSocket;

namespace Scruffy.Services.Discord;

/// <summary>
/// Message component base module
/// </summary>
public class MessageComponentCommandModule
{
    /// <summary>
    /// Custom id
    /// </summary>
    public string CustomId { get; internal set; }

    /// <summary>
    /// Active Component
    /// </summary>
    public SocketMessageComponent Component { get; internal set; }
}