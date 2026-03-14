using Discord;

namespace Scruffy.Services.Discord;

/// <summary>
/// Data of a select menu entry
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public class SelectMenuEntryData<TData>
{
    /// <summary>
    /// Command text
    /// </summary>
    public string CommandText { get; set; }

    /// <summary>
    /// Emote
    /// </summary>
    public IEmote Emote { get; set; }

    /// <summary>
    /// Function to react with an interaction
    /// </summary>
    public Func<IDiscordInteraction, Task<TData>> InteractionResponse { get; set; }

    /// <summary>
    /// Function to react with a standard message
    /// </summary>
    public Func<Task<TData>> Response { get; set; }
}