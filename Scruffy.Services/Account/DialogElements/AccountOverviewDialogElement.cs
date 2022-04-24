using Discord;
using Discord.Interactions;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Services.Account.Modals;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Account.DialogElements;

/// <summary>
/// Account overview
/// </summary>
public class AccountOverviewDialogElement : DialogEmbedSelectMenuElementBase<bool>
{
    #region Fields

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
    /// <param name="repositoryFactory">Repository factory</param>
    public AccountOverviewDialogElement(LocalizationService localizationService, RepositoryFactory repositoryFactory)
        : base(localizationService)
    {
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region DialogEmbedSelectMenuElementBase<bool>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override Task<EmbedBuilder> GetMessage()
    {
        var builder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("AccountConfiguration", "Account configuration"))
                                        .WithDescription(LocalizationGroup.GetText("Description", "With the following assistant you will be able to edit all your account data."))
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithColor(Color.Green)
                                        .WithTimestamp(DateTime.Now);

        // Guild Wars 2
        var fieldBuilder = new StringBuilder();

        fieldBuilder.AppendLine($"{DiscordEmoteService.GetGuildWars2Emote(CommandContext.Client)} {Format.Bold("Guild Wars 2")}");

        var discordAccountsQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                     .GetQuery()
                                                     .Select(obj => obj);

        var guildWarsAccounts = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                  .GetQuery()
                                                  .Where(obj => discordAccountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                                              && obj2.Id == CommandContext.User.Id))
                                                  .Select(obj => obj.Name)
                                                  .ToList();

        if (guildWarsAccounts.Count > 0)
        {
            foreach (var guildWarsAccount in guildWarsAccounts)
            {
                fieldBuilder.AppendLine($"> {guildWarsAccount}");
            }
        }
        else
        {
            fieldBuilder.AppendLine("> -");
        }

        fieldBuilder.AppendLine(string.Empty);

        // GW2 DPS Reports
        fieldBuilder.AppendLine(Format.Bold("GW2 DPS Reports"));

        var userData = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.Id == CommandContext.User.Id)
                                         .Select(obj => new
                                                        {
                                                            obj.User.GitHubAccount,
                                                            obj.User.DpsReportUserToken,
                                                            obj.User.IsDataStorageAccepted,
                                                            obj.User.IsExtendedDataStorageAccepted,
                                                            obj.User.Name,
                                                            obj.User.Birthday
                                                        })
                                         .FirstOrDefault();

        fieldBuilder.AppendLine($"> {userData?.DpsReportUserToken ?? "-"}");
        fieldBuilder.AppendLine(string.Empty);

        // GitHub
        fieldBuilder.AppendLine($"{DiscordEmoteService.GetGitHubEmote(CommandContext.Client)} {Format.Bold("GitHub")}");
        fieldBuilder.AppendLine($"> {userData?.GitHubAccount ?? "-"}");
        fieldBuilder.Append("\u200b");
        builder.AddField(LocalizationGroup.GetText("Accounts", "Accounts"), fieldBuilder.ToString(), true);

        // Personal Data
        fieldBuilder.Clear();
        fieldBuilder.AppendLine(Format.Bold(LocalizationGroup.GetText("Name", "Name")));
        fieldBuilder.AppendLine($"> {userData?.Name ?? "-"}");
        fieldBuilder.AppendLine(string.Empty);
        fieldBuilder.AppendLine(Format.Bold(LocalizationGroup.GetText("Birthday", "Birthday")));
        fieldBuilder.AppendLine($"> {userData?.Birthday?.ToString("d", LocalizationGroup.CultureInfo) ?? "-"}");
        builder.AddField(LocalizationGroup.GetText("PersonalData", "Personal data"), fieldBuilder.ToString(), true);

        // Data storage terms
        fieldBuilder.Clear();
        fieldBuilder.AppendLine(Format.Bold(LocalizationGroup.GetText("General", "General")));
        fieldBuilder.AppendLine($"> {(userData?.IsDataStorageAccepted ?? false ? DiscordEmoteService.GetCheckEmote(CommandContext.Client) : DiscordEmoteService.GetCrossEmote(CommandContext.Client))}");
        fieldBuilder.AppendLine(string.Empty);
        fieldBuilder.AppendLine(Format.Bold(LocalizationGroup.GetText("ExtendedGuildStatistics", "Extended guild statistics")));
        fieldBuilder.AppendLine($"> {(userData?.IsExtendedDataStorageAccepted ?? false ? DiscordEmoteService.GetCheckEmote(CommandContext.Client) : DiscordEmoteService.GetCrossEmote(CommandContext.Client))}");
        builder.AddField(LocalizationGroup.GetText("DataProcessingTerms", "Data processing terms"), fieldBuilder.ToString(), true);

        return Task.FromResult(builder);
    }

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public override string GetPlaceholder() => LocalizationGroup.GetText("Placeholder", "Please choose which data do you like to edit...");

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<SelectMenuEntryData<bool>> GetEntries() => new List<SelectMenuEntryData<bool>>
                                                                             {
                                                                                 new()
                                                                                 {
                                                                                     CommandText = "Guild Wars 2",
                                                                                     Emote = DiscordEmoteService.GetGuildWars2Emote(CommandContext.Client),
                                                                                     Response = RunSubElement<AccountGuildWarsConfigurationDialogElement, bool>
                                                                                 },
                                                                                 new()
                                                                                 {
                                                                                     CommandText = "GW2 DPS Report",
                                                                                     Emote = DiscordEmoteService.GetDpsReportEmote(CommandContext.Client),
                                                                                     InteractionResponse = async obj =>
                                                                                                           {
                                                                                                               await obj.RespondWithModalAsync<DpsReportUserTokenModal>(DpsReportUserTokenModal.CustomId)
                                                                                                                        .ConfigureAwait(false);

                                                                                                               return false;
                                                                                                           }
                                                                                 },
                                                                                 new()
                                                                                 {
                                                                                     CommandText = "GitHub",
                                                                                     Emote = DiscordEmoteService.GetGitHubEmote(CommandContext.Client),
                                                                                     InteractionResponse = async obj =>
                                                                                                           {
                                                                                                               await obj.RespondWithModalAsync<GitHubAccountModal>(GitHubAccountModal.CustomId)
                                                                                                                        .ConfigureAwait(false);

                                                                                                               return false;
                                                                                                           }
                                                                                 },
                                                                                 new()
                                                                                 {
                                                                                     CommandText = LocalizationGroup.GetText("PersonalData", "Personal data"),
                                                                                     Emote = new Emoji("👤"),
                                                                                     InteractionResponse = async obj =>
                                                                                                           {
                                                                                                               await obj.RespondWithModalAsync<PersonalDataModal>(PersonalDataModal.CustomId)
                                                                                                                        .ConfigureAwait(false);

                                                                                                               return false;
                                                                                                           }
                                                                                 },
                                                                                 new()
                                                                                 {
                                                                                     CommandText = LocalizationGroup.GetText("DataProcessing", "Data processing"),
                                                                                     Emote = new Emoji("🛡"),
                                                                                     Response = async () =>
                                                                                                {
                                                                                                    var isExtendedDataStorageAccepted = false;

                                                                                                    var isDataStorageAccepted = await RunSubElement<AccountDataProcessingTermsDialogElement, bool>().ConfigureAwait(false);
                                                                                                    if (isDataStorageAccepted)
                                                                                                    {
                                                                                                        isExtendedDataStorageAccepted = await RunSubElement<AccountGuildStatisticsTermsDialogElement, bool>().ConfigureAwait(false);
                                                                                                    }

                                                                                                    var discordQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                                                                                                         .GetQuery()
                                                                                                                                         .Select(obj => obj);

                                                                                                    if (_repositoryFactory.GetRepository<UserRepository>()
                                                                                                                          .Refresh(obj => discordQuery.Any(obj2 => obj2.UserId == obj.Id
                                                                                                                                                                && obj2.Id == CommandContext.User.Id),
                                                                                                                                   obj =>
                                                                                                                                   {
                                                                                                                                       obj.IsDataStorageAccepted = isDataStorageAccepted;
                                                                                                                                       obj.IsExtendedDataStorageAccepted = isExtendedDataStorageAccepted;
                                                                                                                                   }) == false)
                                                                                                    {
                                                                                                        throw _repositoryFactory.LastError;
                                                                                                    }

                                                                                                    if (isDataStorageAccepted == false)
                                                                                                    {
                                                                                                        await CommandContext.Channel
                                                                                                                            .SendMessageAsync(LocalizationGroup.GetText("DataDeletion", "A request to delete all of your data was send and will be executed soon."))
                                                                                                                            .ConfigureAwait(false);
                                                                                                    }

                                                                                                    return isDataStorageAccepted;
                                                                                                }
                                                                                 },
                                                                             };

    /// <summary>
    /// Default case if none of the given buttons is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc() => false;

    #endregion // DialogEmbedSelectMenuElementBase<bool>
}