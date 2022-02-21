using System.Net;
using System.Net.Http;

using Discord;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Account.DialogElements;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions.WebApi;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account;

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
        if (commandContextContainer.Message.Channel is not IDMChannel)
        {
            await commandContextContainer.Message
                                         .ReplyAsync(LocalizationGroup.GetText("SwitchToPrivate", "I answered your command as a private message."))
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
                var connector = new GuidWars2ApiConnector(apiKey);
                await using (connector.ConfigureAwait(false))
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
                                                           obj.Permissions = GuildWars2ApiDataConverter.ToPermission(tokenInformation.Permissions);
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
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
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
        if (commandContextContainer.Message.Channel is not IDMChannel)
        {
            await commandContextContainer.Message
                                         .ReplyAsync(LocalizationGroup.GetText("SwitchToPrivate", "I answered your command as a private message."))
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
        if (commandContextContainer.Message.Channel is not IDMChannel)
        {
            await commandContextContainer.Message
                                         .ReplyAsync(LocalizationGroup.GetText("SwitchToPrivate", "I answered your command as a private message."))
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
                    var dialogHandler = new DialogHandler(commandContextContainer);
                    await using (dialogHandler.ConfigureAwait(false))
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

    /// <summary>
    /// Editing a existing account
    /// </summary>
    /// <param name="commandContextContainer">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Validate(CommandContextContainer commandContextContainer)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var invalidAccounts = new List<(string Name, GuildWars2ApiPermission Permissions)>();

            var accounts = dbFactory.GetRepository<GuildWarsAccountRepository>()
                                    .GetQuery()
                                    .Select(obj => new
                                    {
                                        obj.Name,
                                        obj.ApiKey
                                    })
                                    .ToList();

            foreach (var account in accounts)
            {
                var connector = new GuidWars2ApiConnector(account.ApiKey);
                await using (connector.ConfigureAwait(false))
                {
                    try
                    {
                        var tokenInformation = await connector.GetTokenInformationAsync()
                                                              .ConfigureAwait(false);

                        if (tokenInformation?.Permissions == null
                         || tokenInformation.Permissions.Contains(TokenInformation.Permission.Account) == false
                         || tokenInformation.Permissions.Contains(TokenInformation.Permission.Characters) == false
                         || tokenInformation.Permissions.Contains(TokenInformation.Permission.Progression) == false)
                        {
                            invalidAccounts.Add((account.Name, GuildWars2ApiDataConverter.ToPermission(tokenInformation?.Permissions)));
                        }
                    }
                    catch (MissingGuildWars2ApiPermissionException)
                    {
                        invalidAccounts.Add((account.Name, GuildWars2ApiPermission.None));
                    }
                }
            }

            if (invalidAccounts.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine(LocalizationGroup.GetText("ListOfInvalidAccounts", "The following accounts are invalid:"));
                sb.Append("```");

                foreach (var (name, permissions) in invalidAccounts)
                {
                    dbFactory.GetRepository<GuildWarsAccountRepository>()
                             .Refresh(obj => obj.Name == name,
                                      obj => obj.Permissions = permissions);

                    sb.Append(name);
                    sb.Append(" (");
                    sb.Append(permissions);
                    sb.AppendLine(")");
                }

                sb.Append("```");

                await commandContextContainer.Channel
                                             .SendMessageAsync(sb.ToString())
                                             .ConfigureAwait(false);
            }
            else
            {
                await commandContextContainer.Channel
                                             .SendMessageAsync(LocalizationGroup.GetText("NoInvalidAccounts", "There are no invalid accounts."))
                                             .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}