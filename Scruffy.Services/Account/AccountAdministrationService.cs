using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions.WebApi;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account;

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
    /// Editing a existing account
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Validate(InteractionContextContainer context)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var invalidPermissions = new List<(string Name, GuildWars2ApiPermission Permissions)>();
            var invalidNames = new List<(string Name, string NewName)>();

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
                var connector = new GuildWars2ApiConnector(account.ApiKey);
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
                            invalidPermissions.Add((account.Name, GuildWars2ApiDataConverter.ToPermission(tokenInformation?.Permissions)));
                        }
                        else
                        {
                            var accountInformation = await connector.GetAccountInformationAsync()
                                                                    .ConfigureAwait(false);
                            if (accountInformation.Name != account.Name)
                            {
                                invalidNames.Add((account.Name, accountInformation.Name));
                            }
                        }
                    }
                    catch (MissingGuildWars2ApiPermissionException)
                    {
                        invalidPermissions.Add((account.Name, GuildWars2ApiPermission.None));
                    }
                }
            }

            if (invalidPermissions.Count > 0
             || invalidNames.Count > 0)
            {
                if (invalidPermissions.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(LocalizationGroup.GetText("ListOfInvalidAccountsPermissions", "The following accounts are invalid or have missing permissions:"));
                    sb.Append("```");

                    foreach (var (name, permissions) in invalidPermissions)
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

                    await context.SendMessageAsync(sb.ToString())
                                 .ConfigureAwait(false);
                }

                if (invalidNames.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(LocalizationGroup.GetText("ListOfInvalidAccountsNewNames", "The following accounts are renamed"));
                    sb.Append("```");

                    foreach (var (name, newName) in invalidNames)
                    {
                        sb.Append(name);
                        sb.Append(" -> ");
                        sb.Append(newName);
                    }

                    sb.Append("```");

                    await context.SendMessageAsync(sb.ToString())
                                 .ConfigureAwait(false);
                }
            }
            else
            {
                await context.ReplyAsync(LocalizationGroup.GetText("NoInvalidAccounts", "There are no invalid accounts."))
                             .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}