using System.Reflection.Emit;

using Discord;

using Osc;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.DpsReports;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Handling log commands
/// </summary>
public class LogBetaCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Importer for dps.report meta data
    /// </summary>
    private readonly DpsReportsMetaImporter _importer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="importer">dps.report meta importer</param>
    public LogBetaCommandHandler(LocalizationService localizationService, UserManagementService userManagementService, DpsReportsMetaImporter importer)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _importer = importer;
    }

    #endregion // Constructor

    #region Commands

    /// <summary>
    /// Imports logs from dps.report
    /// </summary>
    /// <param name="context">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Import(IContextContainer context)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                               .ConfigureAwait(false);

        await _importer.Import(user.Id)
                       .ConfigureAwait(false);

        await PostStatistics(context, user.Id).ConfigureAwait(false);
    }

    #endregion // Commands

    #region Methods

    /// <summary>
    /// Post log statistics
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="userId">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task PostStatistics(IContextContainer context, long userId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var lastImport = dbFactory.GetRepository<UserDpsReportsConfigurationRepository>()
                                      .GetQuery()
                                      .Where(configuration => configuration.UserId == userId)
                                      .Select(configuration => configuration.LastImport)
                                      .FirstOrDefault();

            var statistics = dbFactory.GetRepository<DpsReportRepository>()
                                      .GetQuery()
                                      .Where(report => report.UserId == userId)
                                      .GroupBy(report => report.UserId)
                                      .Select(group => new
                                                       {
                                                           Count = group.Count(),
                                                           Failed = group.Count(report => report.IsSuccess == false),
                                                           Success = group.Count(report => report.IsSuccess),
                                                           ChallengeModes = group.Count(report => report.Mode == DpsReportMode.ChallengeMode || report.Mode == DpsReportMode.LegendaryChallengeMode)
                                                       })
                                      .FirstOrDefault();

            var statisticsEmbed = new EmbedBuilder().WithColor(Color.DarkBlue)
                                                    .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                    .WithTitle(LocalizationGroup.GetText("LogsStatisticsTitle", "Logs statistics"))
                                                    .WithTimestamp(DateTime.Now)
                                                    .AddField(LocalizationGroup.GetText("LastUpload", "Last upload"), lastImport?.ToString("g", LocalizationGroup.CultureInfo))
                                                    .AddField(LocalizationGroup.GetText("LogCount", "Log count"), statistics?.Count)
                                                    .AddField(LocalizationGroup.GetText("LogFailed", "Failed"), statistics?.Failed)
                                                    .AddField(LocalizationGroup.GetText("LogSuccess", "Succeeded"), statistics?.Success)
                                                    .AddField(LocalizationGroup.GetText("LogChallengeModes", "Challenge Modes"), statistics?.ChallengeModes);

            await context.SendMessageAsync(embed: statisticsEmbed.Build())
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}