using System.Collections;
using System.Globalization;

using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Data.Json.DpsReport;
using Scruffy.Data.Services.DpsReport;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Embed Builder extension for DPS reports
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
    /// Prints the logs of the given type + day
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="type">Type</param>
    /// <param name="dayString">Day</param>
    /// <param name="summarize">Whether to summarize or output all logs</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostLogs(IContextContainer context, DpsReportType type, string dayString, bool summarize)
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
            var uploads = await _dpsReportConnector.GetUploads(
                filter: (Upload upload) =>
                {
                    var encounterDay = DateOnly.FromDateTime(upload.EncounterTime);
                    return encounterDay == day && ((type == DpsReportType.All && _dpsReportConnector.GetReportType(upload.Encounter.BossId) != DpsReportType.Other) || type == _dpsReportConnector.GetReportType(upload.Encounter.BossId));
                },
                shouldAbort: (Upload upload) =>
                {
                    var uploadDay = DateOnly.FromDateTime(upload.UploadTime);
                    return uploadDay < day;
                },
                tokens.ToArray()
            ).ConfigureAwait(false);

            if (uploads.Count > 0)
            {
                var embedBuilder = new DpsReportEmbedBuilder();

                embedBuilder.WithColor(Color.Green)
                            .WithAuthor($"{context.User.Username} - {userName}", context.User.GetAvatarUrl())
                            .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", day.ToString("d", LocalizationGroup.CultureInfo)));

                await AddReports(embedBuilder, uploads, summarize, type == DpsReportType.All, false).ConfigureAwait(false);

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
    /// Prints the guild raid summary of the raid appointments in a week
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostGuildRaidSummary(IContextContainer context)
    {
        var now = DateTime.Now;
        var startOfWeek = now;

        // Go back to monday, the start of the week
        while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
        {
            startOfWeek = startOfWeek.AddDays(-1);
        }

        startOfWeek = startOfWeek.Date;

        // Search for appointments this week
        var appointments = _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                            .GetQuery()
                            .Where(obj => obj.TimeStamp > startOfWeek && obj.TimeStamp < now)
                            .Select(obj => new { obj.Id, obj.TimeStamp })
                            .ToList();

        // Search for appointments of last week
        if (appointments.Count == 0)
        {
            var endOfWeek = startOfWeek.AddMinutes(-1);
            startOfWeek = startOfWeek.AddDays(-7);

            appointments = _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                            .GetQuery()
                            .Where(obj => obj.TimeStamp > startOfWeek && obj.TimeStamp < endOfWeek)
                            .Select(obj => new { obj.Id, obj.TimeStamp })
                            .ToList();
        }

        var tasks = new List<Task<IEnumerable<Upload>>>();

        foreach (var appointment in appointments)
        {
            tasks.Add(GetLogsForGuildRaidDay(appointment.Id));
        }

        var uploads = new List<Upload>();

        foreach (var task in tasks)
        {
            uploads.AddRange(await task.ConfigureAwait(false));
        }

        if (uploads.Count > 0)
        {
            var week = $"{startOfWeek:dd.} - {startOfWeek.AddDays(7):dd.MM.yy}";

            var summaryBuilder = new DpsReportEmbedBuilder();
            var statsBuilder = new DpsReportEmbedBuilder();

            summaryBuilder.WithColor(Color.DarkPurple)
                        .WithTitle(LocalizationGroup.GetFormattedText("DpsReportGuildRaidSummaryTitle", "Guild raid summary ({0})", week));

            var knownGroups = await AddReports(summaryBuilder, uploads, true, false, true).ConfigureAwait(false);

            statsBuilder.WithColor(new Color(160, 132, 23))
                        .WithTitle(LocalizationGroup.GetFormattedText("DpsReportGuildRaidStatsTitle", "Guild raid stats ({0})", week));

            AddGroupStats(statsBuilder, knownGroups);

            await context.ReplyAsync(embeds: new Embed[]
            {
                statsBuilder.Build(),
                summaryBuilder.Build(),
            })
            .ConfigureAwait(false);
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("NoDpsReportUploads", "No DPS-Reports found!"))
                             .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Retrieves DPS reports for the given appointment
    /// </summary>
    /// <param name="appointmentId">ID of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task<IEnumerable<Upload>> GetLogsForGuildRaidDay(long appointmentId)
    {
        var appointment = _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                            .GetQuery()
                            .Where(obj => obj.Id == appointmentId)
                            .Select(obj => new
                            {
                                StartTime = obj.TimeStamp,
                                Tokens = obj.RaidRegistrations
                                                            .Where(obj2 => obj2.LineupExperienceLevelId != null && obj2.User.DpsReportUserToken != null)
                                                            .Select(obj2 => obj2.User.DpsReportUserToken)
                            })
                            .FirstOrDefault();

        var startDate = new DateTimeOffset(appointment.StartTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow)).AddMinutes(-15);
        var endDate = startDate.AddHours(3).AddMinutes(30);

        // Find all raid logs for the raid period
        var uploads = await _dpsReportConnector.GetUploads(
            filter: (Upload upload) => _dpsReportConnector.GetReportType(upload.Encounter.BossId) == DpsReportType.Raid && upload.EncounterTime >= startDate && upload.EncounterTime <= endDate,
            shouldAbort: (Upload upload) => upload.UploadTime < startDate,
            appointment.Tokens.ToArray()
        ).ConfigureAwait(false);

        return uploads.Distinct();
    }

    /// <summary>
    /// Adds the given DPS reports to the given embed builder
    /// </summary>
    /// <param name="embedBuilder">Embed Builder</param>
    /// <param name="uploads">DPS report uploads</param>
    /// <param name="summarize">Whether to summarize or output all logs</param>
    /// <param name="addSubTitles">Whether sub titles should be added</param>
    /// <param name="withStats">Whether to add group stats</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<HashSet<PlayerGroup>> AddReports(DpsReportEmbedBuilder embedBuilder, IEnumerable<Upload> uploads, bool summarize, bool addSubTitles, bool withStats)
    {
        var knownGroups = new HashSet<PlayerGroup>();

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

            foreach (var reportGroup in typeGroup.GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId)))
            {
                var isFractal = reportGroup.Key.GetReportType() == DpsReportType.Fractal;
                var hasMultipleGroups = GroupUploads(ref knownGroups, reportGroup, withStats).Count() > 1;

                var reports = new StringBuilder();

                foreach (var bossGroup in reportGroup.GroupBy(obj => new { obj.Encounter.BossId, obj.Encounter.Boss }))
                {
                    var bossIcon = DiscordEmoteService.GetGuildEmote(_client, _dpsReportConnector.GetRaidBossIconId(bossGroup.Key.BossId)).ToString();

                    var hasNormalTries = isFractal || bossGroup.Any(obj => !obj.Encounter.IsChallengeMode);
                    var hasChallengeTries = !isFractal && bossGroup.Any(obj => obj.Encounter.IsChallengeMode);

                    foreach (var boss in bossGroup.GroupBy(obj => obj.Encounter.IsChallengeMode).OrderBy(obj => obj.Key))
                    {
                        var groupedUploads = GroupUploads(ref knownGroups, boss, withStats);

                        // Start a new field, when we don't have enough space for some logs
                        if (reports.Length > 896)
                        {
                            embedBuilder.AddReportGroup(reportGroup.Key, reports.ToString());
                            reports = new StringBuilder();
                        }

                        var title = new StringBuilder();

                        if (isFractal || !boss.Key)
                        {
                            title.Append(bossIcon);
                            title.Append(' ');
                            title.Append(bossGroup.Key.Boss);
                        }
                        else
                        {
                            title.Append(" └ CM");
                        }

                        if (!hasMultipleGroups)
                        {
                            var fails = GetFailsText(boss);

                            if (!string.IsNullOrEmpty(fails))
                            {
                                title.AppendLine();
                                title.Append(fails);
                            }
                        }

                        reports.AppendLine(title.ToString());

                        foreach (var groupUploads in groupedUploads)
                        {
                            var hasSuccessTry = summarize && groupUploads.Value.Any(obj => obj.Encounter.Success);

                            // Write the group as a sub title
                            if (hasMultipleGroups)
                            {
                                if (hasChallengeTries)
                                {
                                    reports.Append(' ');
                                }

                                reports.Append(" └ ");
                                reports.Append(LocalizationGroup.GetFormattedText("DpsReportPlayerGroup", "Group {0}", groupUploads.Key.ID));

                                var fails = GetFailsText(groupUploads.Value);

                                if (!string.IsNullOrEmpty(fails))
                                {
                                    reports.AppendLine();
                                    reports.Append(' ');
                                    reports.Append(fails);
                                }

                                reports.AppendLine();
                            }

                            if (withStats)
                            {
                                foreach (var upload in groupUploads.Value)
                                {
                                    groupUploads.Key.Stats?.AddEncounter(upload);
                                }
                            }

                            // Enrich the logs with the remaining health
                            var fullLogs = await LoadRemainingHealths(groupUploads.Value, (Upload upload) => summarize && hasSuccessTry && !upload.Encounter.Success).ConfigureAwait(false);

                            // Optionally filter the logs
                            IEnumerable<Tuple<Upload, double?>> filteredLogs = fullLogs.Count > 0 && summarize && !hasSuccessTry
                                ? fullLogs.OrderBy(obj => obj.Item2).Take(3).OrderByDescending(obj => obj.Item2)
                                : fullLogs;

                            foreach (var upload in filteredLogs)
                            {
                                var duration = upload.Item1.Encounter.Duration.ToString(@"mm\:ss");
                                var percentage = string.Empty;

                                if (upload.Item2 != null)
                                {
                                    percentage = $" {Math.Floor(upload.Item2.Value)}% -";
                                }

                                var line = $"{(hasMultipleGroups ? " " : string.Empty)}{(hasChallengeTries ? " " : string.Empty)} └ {(upload.Item1.Encounter.Success ? successIcon : failureIcon)}{percentage} {Format.Url($"{duration} ⧉", upload.Item1.Permalink)}";

                                // Start a new field, when we would reach the character limit
                                if (reports.Length + line.Length > 1024)
                                {
                                    embedBuilder.AddReportGroup(reportGroup.Key, reports.ToString());
                                    reports = new StringBuilder();

                                    if (boss.Key || !hasNormalTries)
                                    {
                                        reports.Append(bossIcon);
                                        reports.Append(' ');
                                        reports.Append(bossGroup.Key.Boss);
                                        reports.AppendLine();
                                    }

                                    reports.AppendLine(title.ToString());
                                }

                                reports.AppendLine(line);
                            }
                        }
                    }
                }

                embedBuilder.AddReportGroup(reportGroup.Key, reports.ToString());
            }
        }

        return knownGroups;
    }

    /// <summary>
    /// Groups the given uploads, using the given known groups.
    /// Adds new groups, if they don't exist.
    /// </summary>
    /// <param name="knownGroups">Previously known groups</param>
    /// <param name="uploads">Uploads</param>
    /// <param name="withStats">Whether to add stats to the groups</param>
    /// <returns>An <see cref="IEnumerable"/> representing the grouped uploads</returns>
    private IEnumerable<KeyValuePair<PlayerGroup, List<Upload>>> GroupUploads(ref HashSet<PlayerGroup> knownGroups, IEnumerable<Upload> uploads, bool withStats)
    {
        var result = new Dictionary<PlayerGroup, List<Upload>>();

        foreach (var upload in uploads)
        {
            var date = DateOnly.FromDateTime(upload.EncounterTime);
            var group = new PlayerGroup(knownGroups.Count + 1, date, upload.Group, withStats);

            // Use the known group, to keep consistent group numbering
            if (knownGroups.TryGetValue(group, out var foundGroup))
            {
                group = foundGroup;
            }
            else
            {
                knownGroups.Add(group);
            }

            if (!result.ContainsKey(group))
            {
                result.Add(group, new());
            }

            result[group].Add(upload);
        }

        return result.OrderBy(obj => obj.Key.ID);
    }

    /// <summary>
    /// Returns a text how many tries where made in the given uploads
    /// </summary>
    /// <param name="uploads">The uploads to determine the try count</param>
    /// <returns>Text how many tries were made, or null on first try</returns>
    private string GetFailsText(IEnumerable<Upload> uploads)
    {
        var fails = uploads.Where(obj => !obj.Encounter.Success);
        var failCount = fails.Count();

        if (failCount > 1)
        {
            var totalDuration = TimeSpan.FromSeconds(fails.Select(obj => obj.Encounter.Duration.TotalSeconds).Sum());
            var result = new StringBuilder();

            result.Append(" │ ");
            result.Append(DiscordEmoteService.GetCrossEmote(_client).ToString());
            result.Append(" (");
            result.Append(failCount);
            result.Append("x)");
            result.Append(" - ");
            result.Append(totalDuration.ToString(totalDuration.TotalHours > 1.0 ? @"hh\:mm\:ss" : @"mm\:ss"));

            return result.ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Loads remaining healths in parallel for given uploads
    /// </summary>
    /// <param name="uploads">The uploads to get the logs for</param>
    /// <param name="shouldSkip">A function to detemine whether a upload should be skipped</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<List<Tuple<Upload, double?>>> LoadRemainingHealths(IEnumerable<Upload> uploads, Func<Upload, bool> shouldSkip)
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
                    result.Add(new(upload, log.RemainingTotalHealth));
                }
                else
                {
                    result.Add(new(upload, null));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Adds the stats of the given groups to the embed builder
    /// </summary>
    /// <param name="embedBuilder">Embed builder</param>
    /// <param name="knownGroups">All known groups</param>
    private void AddGroupStats(DpsReportEmbedBuilder embedBuilder, HashSet<PlayerGroup> knownGroups)
    {
        var successIcon = DiscordEmoteService.GetCheckEmote(_client).ToString();
        var failureIcon = DiscordEmoteService.GetCrossEmote(_client).ToString();
        var dpsIcon = DiscordEmoteService.GetDamageDealerEmote(_client).ToString();

        foreach (var groups in knownGroups.GroupBy(obj => obj.Date))
        {
            var hasMultipleGroups = groups.Count() > 1;
            var content = new StringBuilder();

            foreach (var group in groups)
            {
                if (hasMultipleGroups)
                {
                    content.Append("- ");
                    content.Append(Format.Italics(LocalizationGroup.GetFormattedText("DpsReportPlayerGroup", "Group {0}", group.ID)));
                    content.AppendLine();
                }

                if (hasMultipleGroups)
                {
                    content.Append(' ');
                }

                content.Append("└ ");
                content.Append(LocalizationGroup.GetFormattedText("DpsReportEncounterStat", "{0} Encounter", group.Stats.SuccessfulEncounters));
                content.AppendLine();

                if (hasMultipleGroups)
                {
                    content.Append(' ');
                }

                content.Append("└ ");
                content.Append("Ø ");
                content.Append(group.Stats.AverageDPS.ToString("n0", LocalizationGroup.CultureInfo.NumberFormat));
                content.Append(' ');
                content.Append(dpsIcon);
                content.AppendLine();

                if (hasMultipleGroups)
                {
                    content.Append(' ');
                }

                content.Append("└ ");
                content.Append(LocalizationGroup.GetFormattedText("DpsReportFightDurationStat", "{0} in {1} fights", group.Stats.FailedEncounterTotalTime.ToString(group.Stats.FailedEncounterTotalTime.TotalHours > 1.0 ? @"hh\:mm\:ss" : @"mm\:ss"), failureIcon));
                content.AppendLine();

                if (hasMultipleGroups)
                {
                    content.Append(' ');
                }

                content.Append("└ ");
                content.Append(LocalizationGroup.GetFormattedText("DpsReportFightDurationStat", "{0} in {1} fights", group.Stats.SuccessfulEncounterTotalTime.ToString(group.Stats.SuccessfulEncounterTotalTime.TotalHours > 1.0 ? @"hh\:mm\:ss" : @"mm\:ss"), successIcon));
                content.AppendLine();

                if (hasMultipleGroups)
                {
                    content.Append(' ');
                }

                content.Append("└ ");
                content.Append(group.Stats.FirstEncounterTime.ToLocalTime().ToString("HH:mm"));
                content.Append(" - ");
                content.Append(group.Stats.LastEncounterTime.ToLocalTime().ToString("HH:mm"));
                content.Append(' ');
                content.Append("\uD83D\uDD57");
                content.AppendLine();
            }

            embedBuilder.AddField(Format.Bold(LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(groups.Key.DayOfWeek)), content.ToString(), true);
        }
    }

    #endregion // Methods
}