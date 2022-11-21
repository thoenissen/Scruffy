using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.Guild.DialogElements;

namespace Scruffy.Services.Guild;

/// <summary>
/// Guild user configuration
/// </summary>
public class GuildUserConfigurationService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// User management
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
    /// <param name="repositoryFactory">Repository factor</param>
    public GuildUserConfigurationService(LocalizationService localizationService,
                                         UserManagementService userManagementService,
                                         RepositoryFactory repositoryFactory)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Configuration of an user
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ConfigureUser(IContextContainer context)
    {
        var member = await DialogHandler.Run<GuildUserConfigurationUserDialogElement, IGuildUser>(context)
                                        .ConfigureAwait(false);

        var user = await _userManagementService.GetUserByDiscordAccountId(member)
                                               .ConfigureAwait(false);

        var guildId = _repositoryFactory.GetRepository<GuildRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.DiscordServerId == member.GuildId)
                                        .Select(obj => obj.Id)
                                        .FirstOrDefault();

        var userConfiguration = _repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                                                  .GetQuery()
                                                  .FirstOrDefault(obj => obj.UserId == user.Id
                                                                      && obj.GuildId == guildId)
                             ?? new GuildUserConfigurationEntity
                                {
                                    GuildId = guildId,
                                    UserId = user.Id
                                };
        var continueEdit = false;

        IUserMessage message = null;

        do
        {
            try
            {
                var builder = new EmbedBuilder();

                builder.WithTitle(LocalizationGroup.GetText("UserConfiguration", "User configuration"));
                builder.WithDescription(LocalizationGroup.GetText("Description", "With the following assistant you are able to change the user configuration. Select one of the following options to change them."));

                builder.AddField(LocalizationGroup.GetText("User", "User"), member.Mention);

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{LocalizationGroup.GetText(nameof(userConfiguration.IsFixedRank), "Excluded from ranking changes")}: {(userConfiguration.IsFixedRank ? DiscordEmoteService.GetCheckEmote(context.Client) : DiscordEmoteService.GetCrossEmote(context.Client))}");
                stringBuilder.AppendLine($"{LocalizationGroup.GetText(nameof(userConfiguration.IsInactive), "Inactive")}: {(userConfiguration.IsInactive ? DiscordEmoteService.GetCheckEmote(context.Client) : DiscordEmoteService.GetCrossEmote(context.Client))}");
                builder.AddField(LocalizationGroup.GetText("Configuration", "Configuration"), stringBuilder);

                var components = context.Interactivity.CreateTemporaryComponentContainer<int>(obj => obj.User.Id == context.User.Id);
                await using (components.ConfigureAwait(false))
                {
                    var componentsBuilder = new ComponentBuilder();
                    var selectMenu = new SelectMenuBuilder().WithCustomId(components.AddSelectMenu(0))
                                                            .WithPlaceholder(LocalizationGroup.GetText("PlaceHolder", "Select the value to be changed..."));

                    selectMenu.AddOption(LocalizationGroup.GetText(nameof(userConfiguration.IsFixedRank), "Excluded from ranking changes"),
                                         nameof(userConfiguration.IsFixedRank));

                    selectMenu.AddOption(LocalizationGroup.GetText(nameof(userConfiguration.IsInactive), "Inactive"),
                                         nameof(userConfiguration.IsInactive));

                    componentsBuilder.WithSelectMenu(selectMenu);

                    if (message == null)
                    {
                        message = await context.SendMessageAsync(embed: builder.Build(),
                                                                 components: componentsBuilder.Build())
                                               .ConfigureAwait(false);
                    }
                    else
                    {
                        await message.ModifyAsync(obj =>
                                                  {
                                                      obj.Embed = builder.Build();
                                                      obj.Components = componentsBuilder.Build();
                                                  })
                                     .ConfigureAwait(false);
                    }

                    components.StartTimeout();

                    var (component, _) = await components.Task
                                                         .ConfigureAwait(false);

                    await component.DeferAsync()
                                   .ConfigureAwait(false);

                    switch (component.Data.Values.FirstOrDefault())
                    {
                        case nameof(userConfiguration.IsFixedRank):
                            {
                                userConfiguration.IsFixedRank = userConfiguration.IsFixedRank == false;
                            }
                            break;
                        case nameof(userConfiguration.IsInactive):
                            {
                                userConfiguration.IsInactive = userConfiguration.IsInactive == false;
                            }
                            break;
                    }
                }

                continueEdit = _repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                                                 .AddOrRefresh(obj => obj.UserId == user.Id
                                                                   && obj.Guild.DiscordServerId == member.GuildId,
                                                               obj =>
                                                               {
                                                                   obj.GuildId = userConfiguration.GuildId;
                                                                   obj.UserId = userConfiguration.UserId;
                                                                   obj.IsInactive = userConfiguration.IsInactive;
                                                                   obj.IsFixedRank = userConfiguration.IsFixedRank;
                                                               });

                if (continueEdit
                 && userConfiguration.IsInactive)
                {
                    var ranks = _repositoryFactory.GetRepository<GuildRankRepository>()
                                                  .GetQuery()
                                                  .OrderByDescending(obj => obj.Order)
                                                  .Select(obj => obj.Id)
                                                  .Take(2)
                                                  .ToList();

                    if (ranks.Count == 2)
                    {
                        var inactiveRankId = ranks[0];
                        var lastRankId = ranks[1];

                        _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                          .Refresh(obj => obj.UserId == userConfiguration.UserId
                                                          && obj.GuildId == userConfiguration.GuildId
                                                          && obj.RankId == inactiveRankId,
                                                   obj =>
                                                   {
                                                       obj.RankId = lastRankId;
                                                       obj.TimeStamp = DateTime.Now;
                                                   });
                    }
                }
            }
            catch (ScruffyTimeoutException)
            {
                continueEdit = false;
            }
            finally
            {
                if (continueEdit == false
                 && message != null)
                {
                    await message.ModifyAsync(obj => obj.Components = new ComponentBuilder().Build())
                                 .ConfigureAwait(false);
                }
            }
        }
        while (continueEdit);
    }

    #endregion // Methods
}