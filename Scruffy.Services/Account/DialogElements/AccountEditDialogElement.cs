﻿using System.Net;
using System.Net.Http;

using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Account.DialogElements;

/// <summary>
/// Editing the account information
/// </summary>
public class AccountEditDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// User management service
    /// </summary>
    private UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    public AccountEditDialogElement(LocalizationService localizationService, UserManagementService userManagementService)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <param name="builder">Builder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Account configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure your Guild Wars 2 account configuration."));

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var name = DialogContext.GetValue<string>("AccountName");

            var data = await dbFactory.GetRepository<AccountRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.User.DiscordAccounts.Any(obj2 => obj2.Id == CommandContext.User.Id)
                                                 && obj.Name == name)
                                      .Select(obj => new
                                                     {
                                                         obj.Name,
                                                         IsApiKeyAvailable = obj.ApiKey != null,
                                                         obj.DpsReportUserToken,
                                                     })
                                      .FirstAsync()
                                      .ConfigureAwait(false);

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{Format.Code(LocalizationGroup.GetText("Name", "Name"))}: {data.Name}");
            stringBuilder.AppendLine($"{Format.Code(LocalizationGroup.GetText("IsApiKeyAvailable", "Api Key"))}: {(data.IsApiKeyAvailable ? DiscordEmoteService.GetCheckEmote(CommandContext.Client) : DiscordEmoteService.GetCrossEmote(CommandContext.Client))}");
            stringBuilder.AppendLine($"{Format.Code(LocalizationGroup.GetText("DpsReportUserToken", "dps.report user token"))}: {(string.IsNullOrWhiteSpace(data.DpsReportUserToken) ? DiscordEmoteService.GetCrossEmote(CommandContext.Client) : data.DpsReportUserToken)}");

            builder.AddField(LocalizationGroup.GetText("Data", "Data"), stringBuilder.ToString());
        }
    }

    /// <summary>
    /// Returns the title of the commands
    /// </summary>
    /// <returns>Commands</returns>
    protected override string GetCommandTitle()
    {
        return LocalizationGroup.GetText("CommandTitle", "Commands");
    }

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<bool>>
                              {
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditApiKeyCommand", "{0} Edit api key", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var success = false;

                                                 var apiKey = await RunSubElement<AccountApiKeyDialogElement, string>().ConfigureAwait(false);

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

                                                                 if (accountInformation.Name == DialogContext.GetValue<string>("AccountName"))
                                                                 {
                                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                                     {
                                                                         var user = await _userManagementService.GetUserByDiscordAccountId(CommandContext.User.Id)
                                                                                                                .ConfigureAwait(false);

                                                                         if (dbFactory.GetRepository<AccountRepository>()
                                                                                      .Refresh(obj => obj.UserId == user.Id
                                                                                                   && obj.Name == accountInformation.Name,
                                                                                               obj =>
                                                                                               {
                                                                                                   obj.ApiKey = apiKey;
                                                                                                   obj.Permissions = GuildWars2ApiDataConverter.ToPermission(tokenInformation.Permissions);
                                                                                               }))
                                                                         {
                                                                             success = true;
                                                                         }
                                                                     }
                                                                 }
                                                                 else
                                                                 {
                                                                     await CommandContext.Channel
                                                                                         .SendMessageAsync(LocalizationGroup.GetText("AccountNameMismatch", "The provided api key doesn't match the current account name."))
                                                                                         .ConfigureAwait(false);
                                                                 }
                                                             }
                                                             else
                                                             {
                                                                 await CommandContext.Channel
                                                                                     .SendMessageAsync(LocalizationGroup.GetText("InvalidToken", "The provided token is invalid or doesn't have the required permissions."))
                                                                                     .ConfigureAwait(false);
                                                             }
                                                         }
                                                     }
                                                     catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                                                     {
                                                         await CommandContext.Channel
                                                                             .SendMessageAsync(LocalizationGroup.GetText("InvalidToken", "The provided token is invalid or doesn't have the required permissions."))
                                                                             .ConfigureAwait(false);
                                                     }
                                                 }

                                                 return success;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEdit2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditDpsReportUserTokenCommand", "{0} Edit dps report user token", DiscordEmoteService.GetEdit2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var token = await RunSubElement<AccountDpsReportUserTokenDialogElement, string>()
                                                                 .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var accountName = DialogContext.GetValue<string>("AccountName");

                                                     var user = await _userManagementService.GetUserByDiscordAccountId(CommandContext.User.Id)
                                                                                            .ConfigureAwait(false);

                                                     dbFactory.GetRepository<AccountRepository>()
                                                              .Refresh(obj => obj.UserId == user.Id
                                                                           && obj.Name == accountName,
                                                                       obj => obj.DpsReportUserToken = token);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                                      Func = () => Task.FromResult(false)
                                  }
                              };
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc()
    {
        return false;
    }

    #endregion DialogReactionElementBase<bool>
}