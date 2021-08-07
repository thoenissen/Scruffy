using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Account.DialogElements;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account
{
    /// <summary>
    /// Account administration
    /// </summary>
    public class AccountAdministrationService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public AccountAdministrationService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Adding a new account
        /// </summary>
        /// <param name="originCommandContext">Original command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Add(CommandContext originCommandContext)
        {
            if (originCommandContext.Message.Channel.IsPrivate == false)
            {
                await originCommandContext.Message
                                          .DeleteAsync()
                                          .ConfigureAwait(false);
            }

            var commandContext = await CommandContextContainer.SwitchToDirectMessageContext(originCommandContext)
                                                              .ConfigureAwait(false);

            var apiKey = await DialogHandler.Run<AccountApiKeyDialogElement, string>(commandContext)
                                            .ConfigureAwait(false);

            apiKey = apiKey?.Trim();

            if (string.IsNullOrWhiteSpace(apiKey) == false)
            {
                await using (var connector = new GuidWars2ApiConnector(apiKey))
                {
                    var tokenInformation = await connector.GetTokenInformationAsync()
                                                          .ConfigureAwait(false);

                    if (tokenInformation?.Permissions != null
                     && tokenInformation.Permissions.Contains(TokenInformation.Permission.Account)
                     && tokenInformation.Permissions.Contains(TokenInformation.Permission.Characters))
                    {
                        var accountInformation = await connector.GetAccountInformationAsync()
                                                                .ConfigureAwait(false);

                        using (var dbFactory = RepositoryFactory.CreateInstance())
                        {
                            if (dbFactory.GetRepository<AccountRepository>()
                                         .AddOrRefresh(obj => obj.UserId == commandContext.User.Id
                                                           && obj.Name == accountInformation.Name,
                                                       obj =>
                                                       {
                                                           obj.UserId = commandContext.User.Id;
                                                           obj.Name = accountInformation.Name;
                                                           obj.ApiKey = apiKey;
                                                       }))
                            {
                                await Edit(commandContext, accountInformation.Name).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        await commandContext.Channel
                                            .SendMessageAsync(LocalizationGroup.GetText("InvalidToken", "The provided token doesn't have the required permissions."))
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Editing a existing account
        /// </summary>
        /// <param name="originCommandContext">Original command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Edit(CommandContext originCommandContext)
        {
            if (originCommandContext.Message.Channel.IsPrivate == false)
            {
                await originCommandContext.Message
                                          .DeleteAsync()
                                          .ConfigureAwait(false);
            }

            var commandContext = await CommandContextContainer.SwitchToDirectMessageContext(originCommandContext)
                                                              .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var names = dbFactory.GetRepository<AccountRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.UserId == commandContext.User.Id)
                                     .Select(obj => obj.Name)
                                     .Take(2)
                                     .ToList();

                var name = names.Count == 1
                               ? names.First()
                               : await DialogHandler.Run<AccountSelectionDialogElement, string>(commandContext)
                                                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(name) == false)
                {
                    await Edit(commandContext, name).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Editing a existing account
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task Edit(ICommandContext commandContext, string name)
        {
            bool repeat;

            do
            {
                repeat = await DialogHandler.Run<AccountEditDialogElement, bool>(commandContext, dialogContext => dialogContext.SetValue("AccountName", name))
                                            .ConfigureAwait(false);
            }
            while (repeat);
        }

        #endregion // Methods
    }
}
