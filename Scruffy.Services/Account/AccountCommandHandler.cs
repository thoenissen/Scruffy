using System.Globalization;
using System.Net;
using System.Net.Http;

using Discord;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Account.DialogElements;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account;

/// <summary>
/// Account command handler
/// </summary>
public class AccountCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="repositoryFactory">Repository factory</param>
    public AccountCommandHandler(LocalizationService localizationService,
                                 UserManagementService userManagementService,
                                 RepositoryFactory repositoryFactory)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region Public methods

    /// <summary>
    /// Configuration of the user account
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ConfigureAccount(IContextContainer context)
    {
        if (context.Channel is not IDMChannel)
        {
            await context.ReplyAsync(LocalizationGroup.GetText("SwitchToPrivate", "I answered your command as a private message."))
                         .ConfigureAwait(false);

            await context.SwitchToDirectMessageContext()
                         .ConfigureAwait(false);
        }

        var userData = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                                   .ConfigureAwait(false);

        if (userData.IsDataStorageAccepted != true)
        {
            userData.IsDataStorageAccepted = await DialogHandler.Run<AccountDataProcessingTermsDialogElement, bool>(context).ConfigureAwait(false);

            if (_userManagementService.SetIsDataStorageAccepted(userData.Id, userData.IsDataStorageAccepted.Value) == false)
            {
                userData.IsDataStorageAccepted = false;
            }
            else if (userData.IsDataStorageAccepted == false)
            {
                await context.Channel
                             .SendMessageAsync(LocalizationGroup.GetText("CommandProcessed", "The command has been processed."))
                             .ConfigureAwait(false);
            }
        }

        if (userData.IsDataStorageAccepted == true)
        {
            await AccountOverview(context).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Add or refresh a Guild Wars 2 account
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="apiKey">API key</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddOrRefreshGuildWarsAccount(InteractionContextContainer context, string apiKey)
    {
        var deferMessage = await context.DeferProcessing()
                                        .ConfigureAwait(false);

        try
        {
            var connector = new GuildWars2ApiConnector(apiKey);
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
                        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                                               .ConfigureAwait(false);

                        if (dbFactory.GetRepository<AccountRepository>()
                                     .AddOrRefresh(obj => obj.UserId == user.Id
                                                       && obj.Name == accountInformation.Name,
                                                   obj =>
                                                   {
                                                       obj.UserId = user.Id;
                                                       obj.Name = accountInformation.Name;
                                                       obj.ApiKey = apiKey;
                                                       obj.Permissions = GuildWars2ApiDataConverter.ToPermission(tokenInformation.Permissions);
                                                   }))
                        {
                            await AccountOverview(context).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await deferMessage.ModifyAsync(obj => obj.Content = LocalizationGroup.GetText("InvalidToken", "The provided token is invalid or doesn't have the required permissions."))
                                      .ConfigureAwait(false);
                }
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            await deferMessage.ModifyAsync(obj => obj.Content = LocalizationGroup.GetText("InvalidToken", "The provided token is invalid or doesn't have the required permissions."))
                              .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Add or refresh DPS Report user token
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="userToken">User token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddOrRefreshDpsReportUserToken(InteractionContextContainer context, string userToken)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        var discordQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                             .GetQuery()
                                             .Select(obj => obj);

        if (_repositoryFactory.GetRepository<UserRepository>()
                              .Refresh(obj => discordQuery.Any(obj2 => obj2.UserId == obj.Id
                                                                    && obj2.Id == context.User.Id),
                                       obj => obj.DpsReportUserToken = string.IsNullOrWhiteSpace(userToken) ? null : userToken))
        {
            await AccountOverview(context).ConfigureAwait(false);
        }
        else
        {
            throw _repositoryFactory.LastError;
        }
    }

    /// <summary>
    /// Add or refresh GitHub account
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="accountName">Account name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddOrRefreshGitHubAccount(InteractionContextContainer context, string accountName)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        var discordQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                             .GetQuery()
                                             .Select(obj => obj);

        if (_repositoryFactory.GetRepository<UserRepository>()
                              .Refresh(obj => discordQuery.Any(obj2 => obj2.UserId == obj.Id
                                                                    && obj2.Id == context.User.Id),
                                       obj => obj.GitHubAccount = string.IsNullOrWhiteSpace(accountName) ? null : accountName))
        {
            await AccountOverview(context).ConfigureAwait(false);
        }
        else
        {
            throw _repositoryFactory.LastError;
        }
    }

    /// <summary>
    /// Refresh personal data
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="name">Name</param>
    /// <param name="birthday">Birthday</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddOrRefreshPersonalData(InteractionContextContainer context, string name, string birthday)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        var discordQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                             .GetQuery()
                                             .Select(obj => obj);

        if (_repositoryFactory.GetRepository<UserRepository>()
                              .Refresh(obj => discordQuery.Any(obj2 => obj2.UserId == obj.Id
                                                                    && obj2.Id == context.User.Id),
                                       obj =>
                                       {
                                           obj.Name = string.IsNullOrWhiteSpace(name) ? null : name;

                                           if (string.IsNullOrWhiteSpace(birthday) == false
                                            && DateTime.TryParseExact(birthday, "yyyy-MM-dd", null, DateTimeStyles.None, out var day))
                                           {
                                               obj.Birthday = day;
                                           }
                                           else
                                           {
                                               obj.Birthday = null;
                                           }
                                       }))
        {
            await AccountOverview(context).ConfigureAwait(false);
        }
        else
        {
            throw _repositoryFactory.LastError;
        }
    }

    #endregion // Public methods

    #region Private methods

    /// <summary>
    /// Account overview
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task AccountOverview(IContextContainer context)
    {
        try
        {
            while (await DialogHandler.Run<AccountOverviewDialogElement, bool>(context)
                                      .ConfigureAwait(false))
            {
            }
        }
        catch (ScruffyTimeoutException)
        {
        }
    }

    #endregion // Private methods
}