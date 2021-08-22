using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Games;

namespace Scruffy.Commands
{
    /// <summary>
    /// Games
    /// </summary>
    [Group("games")]
    public class GamesCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GamesCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Counter

        /// <summary>
        /// Counter game
        /// </summary>
        [Group("counter")]
        public class GamesCounterCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public GamesCounterCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("add")]
            [RequireGuild]
            [RequireAdministratorPermissions]
            public Task Add(CommandContext commandContext)
            {
                return InvokeAsync(commandContext, async commandContextContainer =>
                                                   {
                                                       if (await CounterGameService.Add(commandContextContainer)
                                                                                   .ConfigureAwait(false))
                                                       {
                                                           await commandContextContainer.Message
                                                                                        .DeleteAsync()
                                                                                        .ConfigureAwait(false);
                                                       }
                                                   });
            }

            /// <summary>
            /// Adds the counter game to the channel
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("remove")]
            [RequireGuild]
            [RequireAdministratorPermissions]
            public Task Remove(CommandContext commandContext)
            {
                return InvokeAsync(commandContext, async commandContextContainer =>
                                                   {
                                                       if (await CounterGameService.Remove(commandContextContainer)
                                                                                   .ConfigureAwait(false))
                                                       {
                                                           await commandContextContainer.Message
                                                                                        .DeleteAsync()
                                                                                        .ConfigureAwait(false);
                                                       }
                                                   });
            }

            #endregion // Methods
        }

        #endregion // Counter

        #region Word chain

        /// <summary>
        /// Word chain game
        /// </summary>
        [Group("wordchain")]
        public class GamesWordChainCommandModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public GamesWordChainCommandModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("add")]
            [RequireGuild]
            [RequireAdministratorPermissions]
            public Task Add(CommandContext commandContext)
            {
                return InvokeAsync(commandContext, async commandContextContainer =>
                {
                    if (await WordChainGameService.Add(commandContextContainer)
                                                  .ConfigureAwait(false))
                    {
                        await commandContextContainer.Message
                                                     .DeleteAsync()
                                                     .ConfigureAwait(false);
                    }
                });
            }

            /// <summary>
            /// Adds the Word chain game to the channel
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("remove")]
            [RequireGuild]
            [RequireAdministratorPermissions]
            public Task Remove(CommandContext commandContext)
            {
                return InvokeAsync(commandContext, async commandContextContainer =>
                {
                    if (await WordChainGameService.Remove(commandContextContainer)
                                                  .ConfigureAwait(false))
                    {
                        await commandContextContainer.Message
                                                     .DeleteAsync()
                                                     .ConfigureAwait(false);
                    }
                });
            }

            #endregion // Methods
        }

        #endregion // Word chain
    }
}
