using Discord;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
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
            var unknownError = new List<(string Name, string Exception)>();

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
                    catch (Exception ex)
                    {
                        unknownError.Add((account.Name, ex.Message));
                    }
                }
            }

            if (invalidPermissions.Count > 0
                || invalidNames.Count > 0
                || unknownError.Count > 0)
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
                        sb.AppendLine(newName);
                    }

                    sb.Append("```");

                    await context.SendMessageAsync(sb.ToString())
                                 .ConfigureAwait(false);
                }

                if (unknownError.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(LocalizationGroup.GetText("ListOfUnknownErrors", "The following accounts could not be checked"));
                    sb.Append("```");

                    foreach (var (name, exception) in unknownError)
                    {
                        sb.Append(name);
                        sb.Append(" (");
                        sb.Append(exception);
                        sb.AppendLine(")");
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

    /// <summary>
    /// Checking unknown users of the given guild
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task CheckUnknownUsers(InteractionContextContainer context)
    {
        await context.DeferAsync()
                     .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var today = DateTime.Today;

            var guildWarsAccounts = dbFactory.GetRepository<GuildWarsAccountRepository>()
                                             .GetQuery()
                                             .Select(obj => obj);

            var unknownUsers = dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.Date == today
                                                   && obj.Guild.DiscordServerId == context.Guild.Id
                                                   && guildWarsAccounts.Any(obj2 => obj2.Name == obj.Name) == false)
                                        .Select(obj => new
                                                       {
                                                           obj.Name,
                                                           obj.JoinedAt
                                                       })
                                        .OrderBy(obj => obj.JoinedAt)
                                        .ToList();

            if (unknownUsers.Count > 0)
            {
                var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("UnknownUsers", "Unknown users"))
                                                     .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                     .WithColor(Color.Green)
                                                     .WithTimestamp(DateTime.Now);

                var stringBuilder = new StringBuilder();

                foreach (var user in unknownUsers)
                {
                    stringBuilder.Append(Format.Bold(user.Name));
                    stringBuilder.Append(" (");
                    stringBuilder.Append(user.JoinedAt?.ToString("g", LocalizationGroup.CultureInfo));
                    stringBuilder.Append(" | ");

                    var days = (DateTime.Now - user.JoinedAt)?.TotalDays.ToString("0");

                    stringBuilder.Append(days);
                    stringBuilder.Append(" ");

                    if (days == "1")
                    {
                        stringBuilder.Append(LocalizationGroup.GetText("Day", "Day"));
                    }
                    else
                    {
                        stringBuilder.Append(LocalizationGroup.GetText("Days", "Days"));
                    }

                    stringBuilder.Append(")");
                    stringBuilder.Append(Environment.NewLine);
                }

                embedBuilder.WithDescription(stringBuilder.ToString());

                await context.ReplyAsync(embed: embedBuilder.Build())
                             .ConfigureAwait(false);
            }
            else
            {
                await context.ReplyAsync(LocalizationGroup.GetText("NoUnknownUsers", "There are no unknown users in the guild."))
                             .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods

}