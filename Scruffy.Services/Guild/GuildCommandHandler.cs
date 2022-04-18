using Discord;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;

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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="bankService">Guild bank service</param>
    /// <param name="emblemService">Guild emblem service</param>
    /// <param name="rankVisualizationService">Guild ranking visualization service</param>
    public GuildCommandHandler(LocalizationService localizationService,
                               GuildBankService bankService,
                               GuildEmblemService emblemService,
                               GuildRankVisualizationService rankVisualizationService)
        : base(localizationService)
    {
        _bankService = bankService;
        _emblemService = emblemService;
        _rankVisualizationService = rankVisualizationService;
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
    /// Post a personal guild ranking overview
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="user">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task PostPersonalRankingOverview(IContextContainer context, IGuildUser user) => _rankVisualizationService.PostPersonalOverview(context, user);

    #endregion // Methods
}