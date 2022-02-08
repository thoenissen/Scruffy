﻿using Discord;

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
    /// Function corresponding to the reaction
    /// </summary>
    public Func<Task<TData>> Func { get; set; }
}