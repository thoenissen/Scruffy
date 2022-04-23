using Discord;
using Discord.Interactions;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Services.Account.Modals;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Account.DialogElements;

/// <summary>
/// Guild Wars account configuration
/// </summary>
public class AccountGuildWarsConfigurationDialogElement : DialogEmbedSelectMenuElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Localization service
    /// </summary>
    private readonly LocalizationService _localizationService;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="userManagementService">User management service</param>
    public AccountGuildWarsConfigurationDialogElement(LocalizationService localizationService,
                                                      RepositoryFactory repositoryFactory,
                                                      UserManagementService userManagementService)
        : base(localizationService)
    {
        _localizationService = localizationService;
        _repositoryFactory = repositoryFactory;
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region DialogEmbedSelectMenuElementBase<bool>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override Task<EmbedBuilder> GetMessage()
    {
        var builder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("Title", "Guild Wars 2 account configuration"))
                                        .WithDescription(LocalizationGroup.GetText("Description", "With the following assistant you will be able to edit your Guild Wars 2 account data."))
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithColor(Color.Green)
                                        .WithTimestamp(DateTime.Now);
        return Task.FromResult(builder);
    }

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public override string GetPlaceholder() => LocalizationGroup.GetText("Placeholder", "Please choose one of the following options...");

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<SelectMenuEntryData<bool>> GetEntries()
    {
        var list = new List<SelectMenuEntryData<bool>>
                   {
                       new()
                       {
                           CommandText = LocalizationGroup.GetText("AddOrRefresh", "Add or refresh an account"),
                           InteractionResponse = async obj =>
                                                 {
                                                     await obj.RespondWithModalAsync<GuildWarsAccountModal>(GuildWarsAccountModal.CustomId).ConfigureAwait(false);

                                                     return false;
                                                 }
                       }
                   };

        var discordAccountsQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                     .GetQuery()
                                                     .Select(obj => obj);

        var guildWarsAccounts = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                  .GetQuery()
                                                  .Where(obj => discordAccountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                                              && obj2.Id == CommandContext.User.Id))
                                                  .Select(obj => obj.Name)
                                                  .ToList();

        foreach (var guildWarsAccount in guildWarsAccounts)
        {
            list.Add(new()
                     {
                         CommandText = LocalizationGroup.GetFormattedText("RemoveAccount", "Remove account '{0}'", guildWarsAccount),
                         Response = async () =>
                                    {
                                        if (await RunSubElement<AccountWantToDeleteDialogElement, bool>(new AccountWantToDeleteDialogElement(_localizationService, guildWarsAccount)).ConfigureAwait(false))
                                        {
                                            var userData = await _userManagementService.GetUserByDiscordAccountId(CommandContext.User.Id)
                                                                                       .ConfigureAwait(false);

                                            if (_repositoryFactory.GetRepository<GuildWarsAccountDailyLoginCheckRepository>()
                                                                  .RemoveRange(obj => obj.Account.UserId == userData.Id
                                                                                   && obj.Name == guildWarsAccount)
                                             && _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                                  .Remove(obj => obj.UserId == userData.Id
                                                                              && obj.Name == guildWarsAccount))
                                            {
                                                await CommandContext.Channel
                                                                    .SendMessageAsync(LocalizationGroup.GetText("AccountDeleted", "Your account has been successfully deleted."))
                                                                    .ConfigureAwait(false);
                                            }
                                            else
                                            {
                                                throw _repositoryFactory.LastError;
                                            }
                                        }

                                        return true;
                                    }
                     });
        }

        return list;
    }

    /// <summary>
    /// Default case if none of the given buttons is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc() => false;

    #endregion // DialogEmbedSelectMenuElementBase<bool>
}