using System.Threading.Tasks;

using DSharpPlus;

using Scruffy.Services.Core;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Building the lfg message
    /// </summary>
    public class RaidMessageBuilder : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private DiscordClient _client;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Discord client</param>
        /// <param name="localizationService">Localization service</param>
        public RaidMessageBuilder(DiscordClient client, LocalizationService localizationService)
            : base(localizationService)
        {
            _client = client;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Refresh the message
        /// </summary>
        /// <param name="configurationId">Id of the configuration</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshMessageAsync(long configurationId)
        {
            // TODO
            await Task.Delay(1).ConfigureAwait(false);
        }

        #endregion // Methods
    }
}
