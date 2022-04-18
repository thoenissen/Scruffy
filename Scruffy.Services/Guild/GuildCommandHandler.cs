using System.Globalization;

using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.Guild.DialogElements;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Services.Guild;

/// <summary>
/// Guild commands
/// </summary>
public class GuildCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Guid bank service
    /// </summary>
    private readonly GuildBankService _bankService;

    /// <summary>
    /// Guild emblem service
    /// </summary>
    private readonly GuildEmblemService _emblemService;

    /// <summary>
    /// Guild ranking visualization service
    /// </summary>
    private readonly GuildRankVisualizationService _rankVisualizationService;

    /// <summary>
    /// Guild configuration service
    /// </summary>
    private readonly GuildConfigurationService _configurationService;

    /// <summary>
    /// Guild special rank service
    /// </summary>
    private readonly GuildSpecialRankService _specialRankService;

    /// <summary>
    /// Worlds service
    /// </summary>
    private readonly WorldsService _worldsService;

    /// <summary>
    /// Export service
    /// </summary>
    private readonly GuildExportService _exportService;

    /// <summary>
    /// Items service
    /// </summary>
    private readonly ItemsService _itemsService;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// Rank service
    /// </summary>
    private readonly GuildRankService _rankService;

    /// <summary>
    /// Guild configuration service
    /// </summary>
    private readonly GuildUserConfigurationService _userConfigurationService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="bankService">Guild bank service</param>
    /// <param name="emblemService">Guild emblem service</param>
    /// <param name="rankVisualizationService">Guild ranking visualization service</param>
    /// <param name="configurationService">Guild configuration service</param>
    /// <param name="specialRankService">Special rank service</param>
    /// <param name="worldsService">Worlds service</param>
    /// <param name="exportService">Export service</param>
    /// <param name="itemsService">Items service</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="rankService">Rank service</param>
    /// <param name="userConfigurationService">User configuration service</param>
    public GuildCommandHandler(LocalizationService localizationService,
                               GuildBankService bankService,
                               GuildEmblemService emblemService,
                               GuildRankVisualizationService rankVisualizationService,
                               GuildConfigurationService configurationService,
                               GuildSpecialRankService specialRankService,
                               WorldsService worldsService,
                               GuildExportService exportService,
                               ItemsService itemsService,
                               RepositoryFactory repositoryFactory,
                               GuildRankService rankService,
                               GuildUserConfigurationService userConfigurationService)
        : base(localizationService)
    {
        _bankService = bankService;
        _emblemService = emblemService;
        _rankVisualizationService = rankVisualizationService;
        _configurationService = configurationService;
        _specialRankService = specialRankService;
        _worldsService = worldsService;
        _exportService = exportService;
        _itemsService = itemsService;
        _repositoryFactory = repositoryFactory;
        _rankService = rankService;
        _userConfigurationService = userConfigurationService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Check the guild bank
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task GuildBankCheck(IContextContainer context) => _bankService.Check(context);

    /// <summary>
    /// Check of all dyes which are stored in the guild bank are unlocked
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task GuildBankUnlocksDyes(IContextContainer context) => _bankService.CheckUnlocksDyes(context);

    /// <summary>
    /// Check of all dyes which are stored in the guild bank are unlocked
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task RandomEmblems(IContextContainer context, int count) => _emblemService.PostRandomGuildEmblems(context, count);

    /// <summary>
    /// Post a guild ranking overview
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task PostRankingOverview(IContextContainer context) => _rankVisualizationService.PostOverview(context);

    /// <summary>
    /// Post a personal guild ranking overview
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="user">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task PostPersonalRankingOverview(IContextContainer context, IGuildUser user) => _rankVisualizationService.PostPersonalOverview(context, user);

    /// <summary>
    /// Create guild configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ConfigureGuild(IContextContainer context) => _configurationService.CreateGuildConfiguration(context);

    /// <summary>
    /// Voice activity roles configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ConfigureVoiceActivityRoles(IContextContainer context)
    {
        var dialogHandler = new DialogHandler(context);
        await using (dialogHandler.ConfigureAwait(false))
        {
            while (await dialogHandler.Run<GuildActivityDiscordVoiceSetupDialogElement, bool>().ConfigureAwait(false))
            {
            }
        }
    }

    /// <summary>
    /// Message activity roles configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ConfigureMessageActivityRoles(IContextContainer context)
    {
        var dialogHandler = new DialogHandler(context);
        await using (dialogHandler.ConfigureAwait(false))
        {
            while (await dialogHandler.Run<GuildActivityDiscordMessageSetupDialogElement, bool>().ConfigureAwait(false))
            {
            }
        }
    }

    /// <summary>
    /// Channel configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ConfigureNotificationChannel(InteractionContextContainer context)
    {
        var processingMessage = await context.DeferProcessing()
                               .ConfigureAwait(false);

        var type = await DialogHandler.Run<GuildNotificationChannelConfigurationSelectDialogElement, GuildNotificationChannelConfigurationSelectDialogElement.ChannelType?>(context)
                                      .ConfigureAwait(false);

        switch (type)
        {
            case GuildNotificationChannelConfigurationSelectDialogElement.ChannelType.SpecialRankNotification:
                {
                    _configurationService.SetNotificationChannel(context, GuildChannelConfigurationType.SpecialRankRankChange);
                }
                break;
            case GuildNotificationChannelConfigurationSelectDialogElement.ChannelType.CalendarReminderNotification:
                {
                    _configurationService.SetNotificationChannel(context, GuildChannelConfigurationType.CalendarReminder);
                }
                break;
            case GuildNotificationChannelConfigurationSelectDialogElement.ChannelType.GuildLogNotification:
                {
                    _configurationService.SetNotificationChannel(context, GuildChannelConfigurationType.GuildLogNotification);
                }
                break;
            case GuildNotificationChannelConfigurationSelectDialogElement.ChannelType.GuildRankChangeNotification:
                {
                    _configurationService.SetNotificationChannel(context, GuildChannelConfigurationType.GuildRankChanges);
                }
                break;
            case GuildNotificationChannelConfigurationSelectDialogElement.ChannelType.MessageOfTheDay:
                {
                    await _configurationService.SetupMotd(context)
                                               .ConfigureAwait(false);
                }
                break;
            case GuildNotificationChannelConfigurationSelectDialogElement.ChannelType.Calendar:
                {
                    await _configurationService.SetupCalendar(context)
                                               .ConfigureAwait(false);
                }
                break;
            case null:
            default:
                break;
        }

        await processingMessage.DeleteAsync()
                               .ConfigureAwait(false);
    }

    /// <summary>
    /// Configure item
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ConfigureItem(IContextContainer context) => _itemsService.ConfigureItem(context);

    /// <summary>
    /// Configure user
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ConfigureUser(IContextContainer context) => _userConfigurationService.ConfigureUser(context);

    /// <summary>
    /// Rank configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ConfigureRanks(IContextContainer context)
    {
        bool repeat;

        do
        {
            repeat = await DialogHandler.Run<GuildRankSetupDialogElement, bool>(context)
                                        .ConfigureAwait(false);
        }
        while (repeat);
    }

    /// <summary>
    /// Special rank configuration
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ConfigureSpecialRanks(IContextContainer context)
    {
        bool repeat;

        do
        {
            repeat = await DialogHandler.Run<GuildSpecialRankSetupDialogElement, bool>(context)
                                        .ConfigureAwait(false);
        }
        while (repeat);
    }

    /// <summary>
    /// Special rank overview
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task PostSpecialRankOverview(IContextContainer context) => _specialRankService.PostOverview(context);

    /// <summary>
    /// Special rank overview
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task PostWorldsOverview(IContextContainer context) => _worldsService.PostWorldsOverview(context);

    /// <summary>
    /// Export login activity
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ExportLoginActivity(IContextContainer context) => _exportService.ExportLoginActivityLog(context);

    /// <summary>
    /// Export representation state
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ExportRepresentation(IContextContainer context) => _exportService.ExportRepresentation(context);

    /// <summary>
    /// Export members
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ExportMembers(IContextContainer context) => _exportService.ExportGuildMembers(context);

    /// <summary>
    /// Export roles
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ExportRoles(IContextContainer context) => _exportService.ExportGuildRoles(context);

    /// <summary>
    /// Export items
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ExportItems(IContextContainer context) => _itemsService.ExportCustomValues(context);

    /// <summary>
    /// Export rank assignments
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task ExportAssignments(IContextContainer context) => _exportService.ExportGuildRankAssignments(context);

    /// <summary>
    /// Export stash
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="mode">Mode</param>
    /// <param name="sinceDate">Since date</param>
    /// <param name="sinceTime">Since time</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ExportStash(IContextContainer context, string mode, string sinceDate, string sinceTime)
    {
        if (DateTime.TryParseExact(sinceDate,
                                   "yyyy-MM-dd",
                                   CultureInfo.InvariantCulture,
                                   DateTimeStyles.None,
                                   out var date))
        {
            if (string.IsNullOrWhiteSpace(sinceTime) == false)
            {
                if (TimeSpan.TryParseExact(sinceTime, "hh\\:mm", null, out var time))
                {
                    date = date.Add(time);
                }
            }

            if (mode == "sum")
            {
                await _exportService.ExportStashLogSummarized(context, date)
                                    .ConfigureAwait(false);
            }
            else
            {
                await _exportService.ExportStashLog(context, date)
                                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Export upgrades
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="mode">Mode</param>
    /// <param name="sinceDate">Since date</param>
    /// <param name="sinceTime">Since time</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ExportUpgrades(IContextContainer context, string mode, string sinceDate, string sinceTime)
    {
        if (DateTime.TryParseExact(sinceDate,
                                   "yyyy-MM-dd",
                                   CultureInfo.InvariantCulture,
                                   DateTimeStyles.None,
                                   out var date))
        {
            if (string.IsNullOrWhiteSpace(sinceTime) == false)
            {
                if (TimeSpan.TryParseExact(sinceTime, "hh\\:mm", null, out var time))
                {
                    date = date.Add(time);
                }
            }

            if (mode == "sum")
            {
                await _exportService.ExportUpgradesLogSummarized(context, date)
                                    .ConfigureAwait(false);
            }
            else
            {
                await _exportService.ExportUpgradesLog(context, date)
                                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Export current points
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="sinceDate">Since date</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ExportCurrentPoints(IContextContainer context, string sinceDate)
    {
        if (DateTime.TryParseExact(sinceDate,
                                   "yyyy-MM-dd",
                                   CultureInfo.InvariantCulture,
                                   DateTimeStyles.None,
                                   out var date))
        {
            await _exportService.ExportGuildRankPoints(context, date)
                                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Check rank assignments
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task CheckRankAssignments(IContextContainer context)
    {
        var embed = await _rankService.CheckCurrentAssignments(_repositoryFactory.GetRepository<GuildRepository>()
                                                                                     .GetQuery()
                                                                                     .Where(obj => obj.DiscordServerId == context.Guild.Id)
                                                                                     .Select(obj => obj.Id)
                                                                                     .First(),
                                                               true)
                                          .ConfigureAwait(false);

        if (embed != null)
        {
            await context.ReplyAsync(embed: embed)
                         .ConfigureAwait(false);
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("NoChangedRequired", "No rank changed are required."))
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}