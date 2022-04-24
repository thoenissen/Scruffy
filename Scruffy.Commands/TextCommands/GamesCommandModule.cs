using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Games
/// </summary>
[Group("games")]
[RequireContext(ContextType.Guild)]
[RequireAdministratorPermissions]
[BlockedChannelCheck]
public class GamesCommandModule : LocatedTextCommandModuleBase
{
    #region Counter

    /// <summary>
    /// Counter game
    /// </summary>
    [Group("counter")]
    [RequireContext(ContextType.Guild)]
    [RequireAdministratorPermissions]
    public class GamesCounterCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Adds the counter game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("add")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Add() => ShowMigrationMessage("games");

        /// <summary>
        /// Adds the counter game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remove")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Remove() => ShowMigrationMessage("games");

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
    public class GamesWordChainCommandModule : LocatedTextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Adds the WordChain game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("add")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Add() => ShowMigrationMessage("games");

        /// <summary>
        /// Adds the Word chain game to the channel
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("remove")]
        [RequireContext(ContextType.Guild)]
        [RequireAdministratorPermissions]
        public Task Remove() => ShowMigrationMessage("games");

        #endregion // Methods
    }

    #endregion // Word chain
}