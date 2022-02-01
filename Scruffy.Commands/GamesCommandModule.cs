using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Games;

namespace Scruffy.Commands;

/// <summary>
/// Games
/// </summary>
[Group("games")]
[RequireContext(ContextType.Guild)]
[RequireAdministratorPermissions]
[BlockedChannelCheck]
public class GamesCommandModule : LocatedCommandModuleBase
{
    #region Counter

    /// <summary>
    /// Counter game
    /// </summary>
    [Group("counter")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class GamesCounterCommandModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Counter game service
        /// </summary>
        public CounterGameService CounterGameService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Adds the counter game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("add")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Add()
        {
            if (await CounterGameService.Add(Context)
                                        .ConfigureAwait(false))
            {
                await Context.Message
                             .DeleteAsync()
                             .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds the counter game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remove")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Remove()
        {
           if (await CounterGameService.Remove(Context)
                                       .ConfigureAwait(false))
           {
               await Context.Message
                            .DeleteAsync()
                            .ConfigureAwait(false);
           }
        }

        #endregion // Methods
    }

    #endregion // Counter

    #region Word chain

    /// <summary>
    /// Word chain game
    /// </summary>
    [Group("wordchain")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class GamesWordChainCommandModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Word chain game service
        /// </summary>
        public WordChainGameService WordChainGameService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Adds the WordChain game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("add")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Add()
        {
            if (await WordChainGameService.Add(Context)
                                          .ConfigureAwait(false))
            {
                await Context.Message
                             .DeleteAsync()
                             .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds the Word chain game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remove")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public async Task Remove()
        {
            if (await WordChainGameService.Remove(Context)
                                          .ConfigureAwait(false))
            {
                await Context.Message
                             .DeleteAsync()
                             .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }

    #endregion // Word chain
}