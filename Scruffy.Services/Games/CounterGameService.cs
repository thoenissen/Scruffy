using System.Threading.Tasks;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Games;
using Scruffy.Data.Enumerations.Games;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Games
{
    /// <summary>
    /// Counter game
    /// </summary>
    public class CounterGameService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CounterGameService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<bool> Add(CommandContextContainer commandContext)
        {
            return Task.Run(() =>
                            {
                                using (var dbFactory = RepositoryFactory.CreateInstance())
                                {
                                    return dbFactory.GetRepository<GameChannelRepository>()
                                                    .AddOrRefresh(obj => obj.DiscordChannelId == commandContext.Channel.Id,
                                                                  obj =>
                                                                  {
                                                                      obj.DiscordChannelId = commandContext.Channel.Id;
                                                                      obj.Type = GameType.Counter;
                                                                  });
                                }
                            });
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<bool> Remove(CommandContextContainer commandContext)
        {
            return Task.Run(() =>
                            {
                                using (var dbFactory = RepositoryFactory.CreateInstance())
                                {
                                    return dbFactory.GetRepository<GameChannelRepository>()
                                                    .Remove(obj => obj.DiscordChannelId == commandContext.Channel.Id
                                                                && obj.Type == GameType.Counter);
                                }
                            });
        }

        #endregion // Methods
    }
}
