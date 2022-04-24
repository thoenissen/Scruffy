using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions.WebApi;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;
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
    /// <param name="commandContextContainer">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Validate(IContextContainer commandContextContainer)
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