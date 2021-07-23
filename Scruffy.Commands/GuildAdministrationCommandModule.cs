using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core;
using Scruffy.Services.GuildAdministration;

namespace Scruffy.Commands
{
    /// <summary>
    /// Administration of the Guild Wars 2 guild
    /// </summary>
    [Group("guild")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class GuildAdministrationCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildAdministrationCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Configuration service
        /// </summary>
        public GuildAdministrationConfigurationService ConfigurationService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Setup guild administration
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task Setup(CommandContext commandContext)
        {
            await ConfigurationService.CreateGuildConfiguration(commandContext)
                                      .ConfigureAwait(false);
        }

        /// <summary>
        /// Setting the notification channel
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setNotificationChannel")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task SetNotificationChannel(CommandContext commandContext)
        {
            await ConfigurationService.SetNotificationChannel(commandContext)
                                      .ConfigureAwait(false);
        }

        #endregion // Methods

    }
}
