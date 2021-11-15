using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace Scruffy.Services.Core.Discord;

/// <summary>
/// Data of a reaction
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public class ReactionData<TData>
{
    /// <summary>
    /// Command text
    /// </summary>
    public string CommandText { get; set; }

    /// <summary>
    /// Emoji
    /// </summary>
    public DiscordEmoji Emoji { get; set; }

    /// <summary>
    /// Function corresponding to the reaction
    /// </summary>
    public Func<Task<TData>> Func { get; set; }
}