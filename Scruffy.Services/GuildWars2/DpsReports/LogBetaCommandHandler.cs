using System.Globalization;

using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.DpsReports;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Extensions;
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
    /// Web application URL
    /// </summary>
    private static readonly string _webbAppUrl = Environment.GetEnvironmentVariable("SCRUFFY_WEBAPP_URL");

    /// <summary>
    /// Formats for parsing an input date
    /// </summary>
    private static readonly string[] _dateFormats = ["dd.MM", "dd.MM.yyyy", "yyyy-MM-dd"];

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Importer for dps.report meta data
    /// </summary>
    private readonly DpsReportsMetaImporter _importer;

    /// <summary>
    /// Report generator
    /// </summary>
    private readonly DpsReportReportGenerator _dpsReportReportGenerator;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="importer">dps.report meta importer</param>
    /// <param name="dpsReportReportGenerator">Generator</param>
    public LogBetaCommandHandler(LocalizationService localizationService, UserManagementService userManagementService, DpsReportsMetaImporter importer, DpsReportReportGenerator dpsReportReportGenerator)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _importer = importer;
        _dpsReportReportGenerator = dpsReportReportGenerator;
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

    /// <summary>
    /// Display logs of current day
    /// </summary>
    /// <param name="context">Context container</param>
    /// <param name="dateString">Date string</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Day(IContextContainer context, string dateString)
    {
        var date = ParseDay(dateString);
        var dayTitle = LocalizationGroup.GetFormattedText("DayTitle", "Logs {0}", date.ToString("d", LocalizationGroup.CultureInfo));
        var message = await context.DeferProcessing(dayTitle,
                                                    LocalizationGroup.GetText("ImportLogs", "Your logs are currently being imported."),
                                                    LocalizationGroup.GetText("DayFooter", $"Visit the [website]({_webbAppUrl}) to get a more detailed summary of your logs."))
                                   .ConfigureAwait(false);

        await ImportLogs(context).ConfigureAwait(false);

        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                               .ConfigureAwait(false);

        var encounters = _dpsReportReportGenerator.GetEncounters(user.Id, date, date.AddDays(1));

        var components = new ContainerBuilder().WithTextDisplay($"# {dayTitle}")
                                               .WithSeparator();

        if (encounters.Count > 0)
        {
            foreach (var encounterGroup in encounters.GroupBy(encounter => encounter.Key.Group).OrderBy(encounter => encounter.Key))
            {
                var entryBuilder = new StringBuilder();

                entryBuilder.Append("# ");
                entryBuilder.Append(encounterGroup.Key.ToEmote());
                entryBuilder.AppendLine(encounterGroup.Key.ToDisplayString());

                foreach (var encounterSubGroup in encounterGroup.GroupBy(group => group.Key.SubGroup))
                {
                    var subGroup = encounterSubGroup.Key.ToDisplayString();

                    if (string.IsNullOrWhiteSpace(subGroup) == false)
                    {
                        entryBuilder.Append("**");
                        entryBuilder.Append(subGroup);
                        entryBuilder.AppendLine("**");
                    }

                    foreach (var encounter in encounterSubGroup.GroupBy(encounter => encounter.Key.Encounter))
                    {
                        entryBuilder.Append("> ");
                        entryBuilder.AppendLine(encounter.Key.ToDisplayString());

                        foreach (var report in encounter.SelectMany(entry => entry.Value))
                        {
                            entryBuilder.Append("> ");
                            entryBuilder.Append(report.IsSuccess ? "<:s:1474776706677215513> " : "<:f:1474776707792896081> ");
                            entryBuilder.Append("[");
                            entryBuilder.Append(report.EncounterTime.ToString("t", LocalizationGroup.CultureInfo));
                            entryBuilder.Append(" ⧉](");
                            entryBuilder.Append(report.PermaLink);
                            entryBuilder.AppendLine(")");
                        }
                    }
                }

                components.WithTextDisplay(entryBuilder.ToString())
                          .WithSeparator();
            }
        }
        else
        {
            components.WithTextDisplay(LocalizationGroup.GetText("NoLogsMessage", "There are no logs available."))
                      .WithSeparator();
        }

        components.WithTextDisplay($"-# {LocalizationGroup.GetText("DayFooter", $"Visit the [website]({_webbAppUrl}) to get a more detailed summary of your logs.")}")
                  .WithAccentColor(Color.DarkPurple);

        await message.ModifyAsync(properties => properties.Components = new ComponentBuilderV2().AddComponent(components).Build())
                     .ConfigureAwait(false);
    }

    #endregion // Commands

    #region Methods

    /// <summary>
    /// Parses the day from the given dayString.
    /// </summary>
    /// <param name="dayString">Day to parse</param>
    /// <returns>Parsed day</returns>
    private DateOnly ParseDay(string dayString)
    {
        if (string.IsNullOrWhiteSpace(dayString) == false
            && DateOnly.TryParseExact(dayString, _dateFormats, null, DateTimeStyles.None, out var day))
        {
            return day;
        }

        return DateOnly.FromDateTime(DateTime.Today.Date);
    }

    /// <summary>
    /// Imports logs from dps.report
    /// </summary>
    /// <param name="context">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task ImportLogs(IContextContainer context)
    {
        var user = await _userManagementService.GetUserByDiscordAccountId(context.User)
                                               .ConfigureAwait(false);

        await _importer.Import(user.Id)
                       .ConfigureAwait(false);
    }

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