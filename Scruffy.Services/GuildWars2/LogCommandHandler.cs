using System.Globalization;

using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Data.Json.DpsReport;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Embed Builder Extensions for DPS-Reports
/// </summary>
public class DpsReportEmbedBuilder : EmbedBuilder
{
    /// <summary>
    /// Amount of report groups in the current row
    /// </summary>
    private int _reportGroupsInRow;

    /// <summary>
    /// Adds a sub title field
    /// </summary>
    /// <param name="title">The title to use</param>
    public void AddSubTitle(string title)
    {
        AddField("\u200b", title);
        _reportGroupsInRow = 0;
    }

    /// <summary>
    /// Add a group of DPS reports
    /// </summary>
    /// <param name="group">The DPS report group</param>
    /// <param name="content">The content of the report group</param>
    public void AddReportGroup(DpsReportGroup group, string content)
    {
        if (_reportGroupsInRow > 1)
        {
            AddField("\u200b", "\u200b", false);
            _reportGroupsInRow = 0;
        }

        AddField(group.AsText(), content, true);
        ++_reportGroupsInRow;
    }
}

/// <summary>
/// Handling log commands
/// </summary>
public class LogCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Formats for parsing an input date
    /// </summary>
    private static readonly string[] _dateFormats = { "dd.MM", "dd.MM.yyyy", "MM-dd", "yyyy-MM-dd" };

    /// <summary>
    /// Discord socket client
    /// </summary>
    private readonly DiscordSocketClient _client;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// DPS-Report connector
    /// </summary>
    private readonly DpsReportConnector _dpsReportConnector;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="client">The Discord client</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="dpsReportConnector">DPS-Report connector</param>
    public LogCommandHandler(LocalizationService localizationService,
                              DiscordSocketClient client,
                              RepositoryFactory repositoryFactory,
                              DpsReportConnector dpsReportConnector)
        : base(localizationService)
    {
        _client = client;
        _repositoryFactory = repositoryFactory;
        _dpsReportConnector = dpsReportConnector;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Logs of the given day
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="type">Type</param>
    /// <param name="dayString">Day</param>
    /// <param name="summarize">Whether to summarize or output all logs</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Logs(IContextContainer context, DpsReportType type, string dayString, bool summarize)
    {
        var pairs = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.Id == context.User.Id)
                                            .Select(obj => new { obj.User.GuildWarsAccounts.FirstOrDefault().Name, obj.User.DpsReportUserToken })
                                            .ToList();

        var tokens = pairs.Select(obj => obj.DpsReportUserToken).Distinct().ToList();
        var userName = pairs.Select(obj => obj.Name).Distinct().FirstOrDefault();

        if (string.IsNullOrWhiteSpace(dayString)
            || DateOnly.TryParseExact(dayString, _dateFormats, null, DateTimeStyles.None, out var day) == false)
        {
            day = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        if (tokens.Count > 0)
        {
            var uploads = new List<Upload>();

            uploads.AddRange(await _dpsReportConnector.GetUploads(
                        filter: (Upload upload) =>
                        {
                            var encounterDay = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(upload.EncounterTime).UtcDateTime);
                            return encounterDay == day && ((type == DpsReportType.All && _dpsReportConnector.GetReportType(upload.Encounter.BossId) != DpsReportType.Other) || type == _dpsReportConnector.GetReportType(upload.Encounter.BossId));
                        },
                        shouldAbort: (Upload upload) =>
                        {
                            var uploadDay = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(upload.UploadTime).UtcDateTime);
                            return uploadDay < day;
                        },
                        tokens.ToArray()
                    ).ConfigureAwait(false));

            if (uploads.Count > 0)
            {
                var embedBuilder = new DpsReportEmbedBuilder();

                embedBuilder.WithColor(Color.Green)
                            .WithAuthor($"{context.User.Username} - {userName}", context.User.GetAvatarUrl())
                            .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", day.ToString("d", LocalizationGroup.CultureInfo)));

                await AddReports(embedBuilder, uploads, summarize, type == DpsReportType.All).ConfigureAwait(false);

                await context.ReplyAsync(embed: embedBuilder.Build())
                             .ConfigureAwait(false);
            }
            else
            {
                await context.ReplyAsync(LocalizationGroup.GetText("NoDpsReportUploads", "No DPS-Reports found!"))
                             .ConfigureAwait(false);
            }
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("NoDpsReportToken", "The are no DPS-Report user tokens assigned to your account."))
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Adds the given DPS reports to the given embed builder
    /// </summary>
    /// <param name="embedBuilder">Embed Builder</param>
    /// <param name="uploads">DPS report uploads</param>
    /// <param name="summarize">Whether to summarize or output all logs</param>
    /// <param name="addSubTitles">Whether sub titles should be added</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task AddReports(DpsReportEmbedBuilder embedBuilder, IEnumerable<Upload> uploads, bool summarize, bool addSubTitles)
    {
        var successIcon = DiscordEmoteService.GetCheckEmote(_client).ToString();
        var failureIcon = DiscordEmoteService.GetCrossEmote(_client).ToString();

        foreach (var typeGroup in uploads.OrderBy(obj => _dpsReportConnector.GetSortValue(obj.Encounter.BossId))
                                        .ThenBy(obj => obj.EncounterTime)
                                        .GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId).GetReportType()))
        {
            if (addSubTitles)
            {
                embedBuilder.AddSubTitle($"> {Format.Bold($"{typeGroup.Key}s")}");
            }

            foreach (var group in typeGroup.GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId)))
            {
                var isFractal = group.Key.GetReportType() == DpsReportType.Fractal;
                var reports = new StringBuilder();

                foreach (var bossGroup in group.GroupBy(obj => new { obj.Encounter.BossId, obj.Encounter.Boss }))
                {
                    var bossIcon = DiscordEmoteService.GetGuildEmote(_client, _dpsReportConnector.GetRaidBossIconId(bossGroup.Key.BossId)).ToString();

                    var hasNormalTries = isFractal || bossGroup.Any(obj => !obj.Encounter.IsChallengeMode);
                    var hasChallengeTries = !isFractal && bossGroup.Any(obj => obj.Encounter.IsChallengeMode);

                    foreach (var boss in bossGroup.GroupBy(obj => obj.Encounter.IsChallengeMode).OrderBy(obj => obj.Key))
                    {
                        var hasSuccessTry = summarize && boss.Any(obj => obj.Encounter.Success);

                        // Start a new field, when we don't have enough space for some logs
                        if (reports.Length > 896)
                        {
                            embedBuilder.AddReportGroup(group.Key, reports.ToString());
                            reports = new StringBuilder();
                        }

                        // Build a fancy title
                        var tryCount = boss.Count();
                        var tries = $"({(tryCount > 1 ? LocalizationGroup.GetFormattedText("DpsReportTries", "{0} tries", tryCount) : LocalizationGroup.GetText("DpsReportFirstTry", "First Try"))})";

                        var title = isFractal || !boss.Key
                            ? $"{bossIcon} {bossGroup.Key.Boss} {tries}"
                            : $" └ CM {tries}";

                        reports.AppendLine(title);

                        // Enrich the logs with the remaining health
                        var fullLogs = await LoadRemainingHealths(boss, (Upload upload) => summarize && hasSuccessTry && !upload.Encounter.Success).ConfigureAwait(false);

                        // Optionally filter the logs
                        IEnumerable<Tuple<Upload, double?>> filteredLogs = fullLogs.Count > 0 && summarize && !hasSuccessTry
                            ? fullLogs.OrderBy(obj => obj.Item2).Take(3).OrderByDescending(obj => obj.Item2)
                            : fullLogs;

                        foreach (var upload in filteredLogs)
                        {
                            var duration = TimeSpan.FromSeconds(upload.Item1.Encounter.Duration).ToString(@"mm\:ss");
                            var percentage = string.Empty;

                            if (upload.Item2 != null)
                            {
                                percentage = $" {Math.Floor(upload.Item2.Value)}% -";
                            }

                            var line = $"{(hasChallengeTries ? " " : String.Empty)} └ {(upload.Item1.Encounter.Success ? successIcon : failureIcon)}{percentage} {Format.Url($"{duration} ⧉", upload.Item1.Permalink)}";

                            // Start a new field, when we would reach the character limit
                            if (reports.Length + line.Length > 1024)
                            {
                                embedBuilder.AddReportGroup(group.Key, reports.ToString());
                                reports = new StringBuilder();

                                if (boss.Key || !hasNormalTries)
                                {
                                    reports.Append(bossIcon);
                                    reports.Append(' ');
                                    reports.Append(bossGroup.Key.Boss);
                                    reports.AppendLine();
                                }

                                reports.AppendLine(title);
                            }

                            reports.AppendLine(line);
                        }
                    }
                }

                embedBuilder.AddReportGroup(group.Key, reports.ToString());
            }
        }
    }

    /// <summary>
    /// Loads remaining healths in parallel for given uploads
    /// </summary>
    /// <param name="uploads">The uploads to get the logs for</param>
    /// <param name="shouldSkip">A function to detemine whether a upload should be skipped</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<List<Tuple<Upload, double?>>> LoadRemainingHealths(IEnumerable<Upload> uploads, Func<Upload, bool> shouldSkip)
    {
        var logs = new Dictionary<string, Task<Log>>();

        foreach (var upload in uploads)
        {
            if (!shouldSkip(upload) && !upload.Encounter.Success && upload.Encounter.JsonAvailable)
            {
                logs.Add(upload.Id, _dpsReportConnector.GetLog(upload.Id));
            }
        }

        var result = new List<Tuple<Upload, double?>>();

        foreach (var upload in uploads)
        {
            if (!shouldSkip(upload))
            {
                if (!upload.Encounter.Success && upload.Encounter.JsonAvailable)
                {
                    var log = await logs[upload.Id].ConfigureAwait(false);
                    result.Add(new Tuple<Upload, double?>(upload, log.RemainingTotalHealth));
                }
                else
                {
                    result.Add(new Tuple<Upload, double?>(upload, null));
                }
            }
        }

        return result;
    }

    #endregion // Methods
}