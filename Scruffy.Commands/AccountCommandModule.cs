using System.Net.Http;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Account;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;

namespace Scruffy.Commands
{
    /// <summary>
    /// Calendar commands
    /// </summary>
    [Group("account")]
    [Aliases("ac")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class AccountCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="userManagementService">User management service</param>
        /// <param name="httpClientFactory">HttpClient-Factory</param>
        public AccountCommandModule(LocalizationService localizationService, UserManagementService userManagementService, IHttpClientFactory httpClientFactory)
            : base(localizationService, userManagementService, httpClientFactory)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Account administration service
        /// </summary>
        public AccountAdministrationService AdministrationService { get; set; }

        /// <summary>
        /// Users service
        /// </summary>
        public UserManagementService UserManagementService { get; set; }

        #endregion // Properties

        #region Command methods

        /// <summary>
        /// Adding an account
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("add")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public Task Add(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await UserManagementService.CheckDiscordAccountAsync(commandContextContainer.User.Id)
                                                              .ConfigureAwait(false);

                                   await AdministrationService.Add(commandContextContainer)
                                                              .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Editing an account
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("edit")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public Task Edit(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await UserManagementService.CheckDiscordAccountAsync(commandContextContainer.User.Id)
                                                              .ConfigureAwait(false);

                                   await AdministrationService.Edit(commandContextContainer)
                                                              .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Remove an account
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("remove")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public Task Remove(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await UserManagementService.CheckDiscordAccountAsync(commandContextContainer.User.Id)
                                                              .ConfigureAwait(false);

                                   await AdministrationService.Remove(commandContextContainer)
                                                              .ConfigureAwait(false);
                               });
        }

        #endregion // Command methods

    }
}
