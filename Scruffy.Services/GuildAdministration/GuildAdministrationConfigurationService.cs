using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.GuildAdministration.DialogElements.Forms;

namespace Scruffy.Services.GuildAdministration
{
    /// <summary>
    /// Configuration of the guild administration
    /// </summary>
    public class GuildAdministrationConfigurationService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildAdministrationConfigurationService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Create a new guild configuration
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateGuildConfiguration(CommandContext commandContext)
        {
            var data = await DialogHandler.RunForm<CreateGuildAdministrationFormData>(commandContext, true)
                                          .ConfigureAwait(false);

            if (data != null)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    dbFactory.GetRepository<GuildRepository>()
                             .AddOrRefresh(obj => obj.DiscordServerId == commandContext.Guild.Id,
                                           obj =>
                                           {
                                               obj.ApiKey = data.ApiKey;
                                               obj.GuildId = data.GuildId;
                                               obj.DiscordServerId = commandContext.Guild.Id;
                                           });
                }
            }
        }

        /// <summary>
        /// Create a new guild configuration
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetNotificationChannel(CommandContext commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<GuildRepository>()
                         .AddOrRefresh(obj => obj.DiscordServerId == commandContext.Guild.Id,
                                       obj => obj.NotificationChannelId = commandContext.Channel.Id);
            }

            await commandContext.Message.DeleteAsync()
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }
}
