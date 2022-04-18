using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.GuildWars2
{
    /// <summary>
    /// Guild Wars 2 related commands
    /// </summary>
    public class GuildWars2CommandHandler : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Quaggan service
        /// </summary>
        private QuagganService _quagganService;

        /// <summary>
        /// Guild Wars 2 update service
        /// </summary>
        private GuildWarsUpdateService _updateService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="quagganService">Quaggan service</param>
        /// <param name="updateService">Guild Wars 2 update service</param>
        public GuildWars2CommandHandler(LocalizationService localizationService, QuagganService quagganService, GuildWarsUpdateService updateService)
            : base(localizationService)
        {
            _quagganService = quagganService;
            _updateService = updateService;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="context">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public Task Quaggan(IContextContainer context) => _quagganService.PostRandomQuaggan(context);

        /// <summary>
        /// Next update
        /// </summary>
        /// <param name="context">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public Task Update(IContextContainer context) => _updateService.PostUpdateOverview(context);

        #endregion // Methods
    }
}