using Discord.Interactions;

using Scruffy.Services.Discord;

namespace Scruffy.Commands.MessageComponents;

/// <summary>
/// Debug message component commands
/// </summary>
public class DebugMessageComponentCommandModule : LocatedInteractionModuleBase
{
    #region Constants

    /// <summary>
    /// Group
    /// </summary>
    public const string Group = "debug";

    /// <summary>
    /// Command ping
    /// </summary>
    public const string CommandPing = "ping";

    #endregion // Constants

    #region Commands

    /// <summary>
    /// Ping
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandPing};*")]
    public async Task Ping(string id)
    {
        await Context.Interaction
                     .DeferAsync()
                     .ConfigureAwait(false);

        await Context.Channel
                     .SendMessageAsync($"Pong {id}!")
                     .ConfigureAwait(false);
    }

    #endregion // Commands
}