using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Account.DialogElements;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account
{
    /// <summary>
    /// Account administration
    /// </summary>
    public class AccountAdministrationService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Localization service
        /// </summary>
        private readonly LocalizationService _localizationService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public AccountAdministrationService(LocalizationService localizationService)
            : base(localizationService)
        {
            _localizationService = localizationService;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Adding a new account
        /// </summary>
        /// <param name="commandContextContainer">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Add(CommandContextContainer commandContextContainer)
        {
            if (commandContextContainer.Message.Channel.IsPrivate == false)
            {
                await commandContextContainer.Message
                                             .RespondAsync(LocalizationGroup.GetText("SwitchToPrivate", "I answered your command as a private message."))
                                             .ConfigureAwait(false);
            }

            await commandContextContainer.SwitchToDirectMessageContext()
                                         .ConfigureAwait(false);

            var apiKey = await DialogHandler.Run<AccountApiKeyDialogElement, string>(commandContextContainer)
                                            .ConfigureAwait(false);

            apiKey = apiKey?.Trim();

            if (string.IsNullOrWhiteSpace(apiKey) == false)
            {
                try
                {
                    await using (var connector = new GuidWars2ApiConnector(apiKey))
                    {
                        var tokenInformation = await connector.GetTokenInformationAsync()
                                                              .ConfigureAwait(false);

                        if (tokenInformation?.Permissions != null
                         && tokenInformation.Permissions.Contains(TokenInformation.Permission.Account)
                         && tokenInformation.Permissions.Contains(TokenInformation.Permission.Characters)
                         && tokenInformation.Permissions.Contains(TokenInformation.Permission.Progression))
                        {
                            var accountInformation = await connector.GetAccountInformationAsync()
                                                                    .ConfigureAwait(false);

                            using (var dbFactory = RepositoryFactory.CreateInstance())
                            {
                                var userId = dbFactory.GetRepository<DiscordAccountRepository>()
                                                       .GetQuery()
                                                       .Where(obj => obj.Id == commandContextContainer.User.Id)
                                                       .Select(obj => obj.UserId)
                                                       .First();

                                if (dbFactory.GetRepository<AccountRepository>()
                                             .AddOrRefresh(obj => obj.UserId == userId
                                                               && obj.Name == accountInformation.Name,
                                                           obj =>
                                                           {
                                                               obj.UserId = userId;
                                                               obj.Name = accountInformation.Name;
                                                               obj.ApiKey = apiKey;
                                                               obj.Permissions = GuildWars2ApiPermissionConverter.ToPermission(tokenInformation.Permissions);
                                                           }))
                                {
                                    await commandContextContainer.Channel
                                                                 .SendMessageAsync(LocalizationGroup.GetText("ApiKeyAdded", "Your API-Key has been added successfully."))
                                                                 .ConfigureAwait(false);
                                }
                                else
                                {
                                    throw dbFactory.LastError;
                                }
                            }
                        }
                        else
                        {
                            await commandContextContainer.Channel
                                                         .SendMessageAsync(LocalizationGroup.GetText("InvalidToken", "The provided token is invalid or doesn't have the required permissions."))
                                                         .ConfigureAwait(false);
                        }
                    }
                }
                catch (WebException ex) when (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await commandContextContainer.Channel
                                                 .SendMessageAsync(LocalizationGroup.GetText("InvalidToken", "The provided token is invalid or doesn't have the required permissions."))
                                                 .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Editing a existing account
        /// </summary>
        /// <param name="commandContextContainer">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Edit(CommandContextContainer commandContextContainer)
        {
            if (commandContextContainer.Message.Channel.IsPrivate == false)
            {
                await commandContextContainer.Message
                                             .RespondAsync(LocalizationGroup.GetText("SwitchToPrivate", "I answered your command as a private message."))
                                             .ConfigureAwait(false);
            }

            await commandContextContainer.SwitchToDirectMessageContext()
                                         .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var names = dbFactory.GetRepository<AccountRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.User.DiscordAccounts.Any(obj2 => obj2.Id == commandContextContainer.User.Id))
                                     .Select(obj => obj.Name)
                                     .Take(2)
                                     .ToList();

                if (names.Count == 0)
                {
                    if (await DialogHandler.Run<AccountWantToAddDialogElement, bool>(commandContextContainer).ConfigureAwait(false))
                    {
                        await Add(commandContextContainer).ConfigureAwait(false);
                    }
                }
                else
                {
                    var name = names.Count == 1
                                   ? names.First()
                                   : await DialogHandler.Run<AccountSelectionDialogElement, string>(commandContextContainer)
                                                        .ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(name) == false)
                    {
                        await Edit(commandContextContainer, name).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Removing an account
        /// </summary>
        /// <param name="commandContextContainer">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Remove(CommandContextContainer commandContextContainer)
        {
            if (commandContextContainer.Message.Channel.IsPrivate == false)
            {
                await commandContextContainer.Message
                                             .RespondAsync(LocalizationGroup.GetText("SwitchToPrivate", "I answered your command as a private message."))
                                             .ConfigureAwait(false);
            }

            await commandContextContainer.SwitchToDirectMessageContext()
                                         .ConfigureAwait(false);

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var names = dbFactory.GetRepository<AccountRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.User.DiscordAccounts.Any(obj2 => obj2.Id == commandContextContainer.User.Id))
                                     .Select(obj => obj.Name)
                                     .Take(2)
                                     .ToList();

                if (names.Count == 0)
                {
                    await commandContextContainer.Channel
                                                 .SendMessageAsync(LocalizationGroup.GetText("NoAccountExisting", "You don't have any accounts configured."))
                                                 .ConfigureAwait(false);
                }
                else
                {
                    var name = names.Count == 1
                                   ? names.First()
                                   : await DialogHandler.Run<AccountSelectionDialogElement, string>(commandContextContainer)
                                                        .ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(name) == false)
                    {
                        await using (var dialogHandler = new DialogHandler(commandContextContainer))
                        {
                            if (await dialogHandler.Run<AccountWantToDeleteDialogElement, bool>(new AccountWantToDeleteDialogElement(_localizationService, name)).ConfigureAwait(false))
                            {
                                var userData = await commandContextContainer.GetCurrentUser()
                                                                            .ConfigureAwait(false);

                                if (dbFactory.GetRepository<GuildWarsAccountDailyLoginCheckRepository>()
                                             .RemoveRange(obj => obj.Account.UserId == userData.Id
                                                         && obj.Name == name)
                                 && dbFactory.GetRepository<GuildWarsAccountRepository>()
                                             .Remove(obj => obj.UserId == userData.Id
                                                         && obj.Name == name))
                                {
                                    await commandContextContainer.Channel
                                                                 .SendMessageAsync(LocalizationGroup.GetText("AccountDeleted", "Your account has been successfully deleted."))
                                                                 .ConfigureAwait(false);
                                }
                                else
                                {
                                    throw dbFactory.LastError;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Editing a existing account
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task Edit(CommandContextContainer commandContext, string name)
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
