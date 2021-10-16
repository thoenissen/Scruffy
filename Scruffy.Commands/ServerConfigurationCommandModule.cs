using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;

namespace Scruffy.Commands
{
    /// <summary>
    /// Configuration the server
    /// </summary>
    [Group("config")]
    [Aliases("co")]
    [RequireAdministratorPermissions]
    public class ServerConfigurationCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="userManagementService">User management service</param>
        public ServerConfigurationCommandModule(LocalizationService localizationService, UserManagementService userManagementService)
            : base(localizationService, userManagementService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Prefix resolving
        /// </summary>
        public PrefixResolvingService PrefixResolvingService { get; set; }

        /// <summary>
        /// Administration service
        /// </summary>
        public AdministrationPermissionsValidationService AdministrationPermissionsValidationService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Set the server prefix
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="prefix">Prefix</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("prefix")]
        [RequireGuild]
        public Task SetPrefix(CommandContext commandContext, string prefix)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   if (string.IsNullOrWhiteSpace(prefix) == false
                                    && prefix.Length > 0
                                    && prefix.Any(char.IsControl) == false)
                                   {
                                       PrefixResolvingService.AddOrRefresh(commandContext.Guild.Id, prefix);

                                       await commandContext.RespondAsync(LocalizationGroup.GetFormattedText("UsingNewPrefix", "I will use the following prefix: {0}", prefix))
                                                           .ConfigureAwait(false);
                                   }
                               });
        }

        /// <summary>
        /// Set the server administration role
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="role">Roles</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("adminRole")]
        [RequireGuild]
        public Task SetAdministrationRole(CommandContext commandContext, DiscordRole role)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   AdministrationPermissionsValidationService.AddOrRefresh(commandContext.Guild.Id, role.Id);

                                   await commandContext.Message.CreateReactionAsync(DiscordEmojiService.GetCheckEmoji(commandContext.Client)).ConfigureAwait(false);
                               });
        }
        #endregion // Methods
    }
}
