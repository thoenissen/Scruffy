using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Games;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Games
/// </summary>
public class GamesSlashCommandModule : SlashCommandModuleBase
{
    #region Enums

    /// <summary>
    /// The type of game
    /// </summary>
    public enum GameType
    {
        Counter,
        WordChain
    }

    /// <summary>
    /// The action to perform on a certain game
    /// </summary>
    public enum GameCommandAction
    {
        Add,
        Remove
    }

    #endregion

    #region Properties

    /// <summary>
    /// Counter game service
    /// </summary>
    public CounterGameService CounterGameService { get; set; }

    /// <summary>
    /// Word chain game service
    /// </summary>
    public WordChainGameService WordChainGameService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Adds the counter game to the channel
    /// </summary>
    /// <param name="type">The type of the mini game to manage.</param>
    /// <param name="action">The action to perform on the mini game.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("games", "Manage mini games")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public async Task Manage(GameType type, GameCommandAction action)
    {
        var message = await Context.DeferProcessing().ConfigureAwait(false);

        if (type == GameType.Counter)
        {
            if ((action == GameCommandAction.Add)
                ? await CounterGameService.Add(Context).ConfigureAwait(false)
                : await CounterGameService.Remove(Context).ConfigureAwait(false))
            {
                await message.DeleteAsync().ConfigureAwait(false);
            }
        }
        else
        {
            if ((action == GameCommandAction.Add)
                ? await WordChainGameService.Add(Context).ConfigureAwait(false)
                : await WordChainGameService.Remove(Context).ConfigureAwait(false))
            {
                await message.DeleteAsync().ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}