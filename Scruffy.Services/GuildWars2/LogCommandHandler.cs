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

    /// <summary>
    /// String to display the success icon
    /// </summary>
    private string SuccessIcon => DiscordEmoteService.GetCheckEmote(_client).ToString();

    /// <summary>
    /// String to display the failure icon
    /// </summary>
    private string FailureIcon => DiscordEmoteService.GetCrossEmote(_client).ToString();

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
    /// Posts the logs of the given type + day
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="type">Type</param>
    /// <param name="dayString">Day</param>
    /// <param name="summarize">Whether to summarize or output all logs</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostLogs(IContextContainer context, DpsReportType type, string dayString, bool summarize)
    {
        var account = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.Id == context.User.Id)
                                        .Select(obj => new { obj.User.GuildWarsAccounts.FirstOrDefault().Name, obj.User.DpsReportUserToken })
                                        .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(dayString)
            || DateOnly.TryParseExact(dayString, _dateFormats, null, DateTimeStyles.None, out var day) == false)
        {
            day = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        if (!string.IsNullOrEmpty(account.DpsReportUserToken))
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
                account.DpsReportUserToken
            ).ConfigureAwait(false);

            if (uploads.Count > 0)
            {
                var builder = new DpsReportEmbedBuilder();

                builder.WithColor(Color.Green)
                       .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                       .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", day.ToString("d", LocalizationGroup.CultureInfo)));

                await WriteReports(builder, uploads, summarize, type == DpsReportType.All).ConfigureAwait(false);

                await context.ReplyAsync(embed: builder.Build())
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
    /// <param name="builder">Embed builder</param>
    /// <param name="uploads">DPS report uploads</param>
    /// <param name="summarize">Whether to summarize or output all logs</param>
    /// <param name="addSubTitles">Whether sub titles should be added</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task WriteReports(DpsReportEmbedBuilder builder, IEnumerable<Upload> uploads, bool summarize, bool addSubTitles)
    {
        var knownGroups = new HashSet<PlayerGroup>();

        var typeGroups = uploads.OrderBy(obj => _dpsReportConnector.GetSortValue(obj.Encounter.BossId))
                                .ThenBy(obj => obj.EncounterTime)
                                .GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId).GetReportType());

        addSubTitles &= typeGroups.Count() > 1;

        foreach (var typeGroup in typeGroups)
        {
            if (addSubTitles)
            {
                builder.AddSubTitle($"> {Format.Bold($"{typeGroup.Key}s")}");
            }

            foreach (var reportGroup in typeGroup.GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId)))
            {
                var isFractal = reportGroup.Key.GetReportType() == DpsReportType.Fractal;
                var hasMultipleGroups = GroupUploads(ref knownGroups, reportGroup, false).Count() > 1;

                var reports = new StringBuilder();

                foreach (var bossGroup in reportGroup.GroupBy(obj => new { obj.Encounter.BossId, obj.Encounter.Boss }))
                {
                    // Start a new field, when we don't have enough space for some logs
                    if (reports.Length > 896)
                    {
                        builder.AddReportGroup(reportGroup.Key, reports.ToString());
                        reports = new StringBuilder();
                    }

                    var hasNormalTries = isFractal || bossGroup.Any(obj => !obj.Encounter.IsChallengeMode);
                    var hasChallengeTries = !isFractal && bossGroup.Any(obj => obj.Encounter.IsChallengeMode);

                    foreach (var boss in bossGroup.GroupBy(obj => obj.Encounter.IsChallengeMode).OrderBy(obj => obj.Key))
                    {
                        var title = BuildTitle(boss, summarize, hasNormalTries, hasChallengeTries);
                        reports.AppendLine(title);

                        foreach (var groupUploads in GroupUploads(ref knownGroups, boss, false))
                        {
                            // Write the group as a sub title
                            if (hasMultipleGroups)
                            {
                                if (hasChallengeTries)
                                {
                                    reports.Append(' ');
                                }

                                reports.Append(" └ ");
                                reports.Append(LocalizationGroup.GetFormattedText("DpsReportPlayerGroup", "Group {0}", groupUploads.Key.Id));
                                WriteFails(reports, groupUploads.Value, summarize, hasNormalTries, hasChallengeTries);
                                reports.AppendLine();
                            }

                            // Enrich the logs with the remaining health
                            var hasSuccessTry = summarize && groupUploads.Value.Any(obj => obj.Encounter.Success);
                            var fullLogs = await LoadRemainingHealths(groupUploads.Value, (Upload upload) => hasSuccessTry && !upload.Encounter.Success).ConfigureAwait(false);

                            WriteLogs(builder, ref reports, title, fullLogs, summarize, hasNormalTries, hasChallengeTries);
                        }
                    }
                }

                builder.AddReportGroup(reportGroup.Key, reports.ToString());
            }
        }
    }

    /// <summary>
    /// Posts the last <paramref name="count"/> of <paramref name="group"/> tries
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="group">Group to search for</param>
    /// <param name="count">Max count logs to search</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostHistory(IContextContainer context, DpsReportGroup group, int count)
    {
        var account = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.Id == context.User.Id)
                                        .Select(obj => new { obj.User.GuildWarsAccounts.FirstOrDefault().Name, obj.User.DpsReportUserToken })
                                        .FirstOrDefault();

        if (!string.IsNullOrEmpty(account.DpsReportUserToken))
        {
            var counts = new Dictionary<int, int>();

            var uploads = await _dpsReportConnector.GetUploads(
                filter: (Upload upload) =>
                {
                    if (upload.Encounter.Success && _dpsReportConnector.GetReportGroup(upload.Encounter.BossId) == group)
                    {
                        if (!counts.ContainsKey(upload.Encounter.BossId))
                        {
                            counts.Add(upload.Encounter.BossId, 0);
                        }

                        ++counts[upload.Encounter.BossId];
                        return true;
                    }

                    return false;
                },
                shouldAbort: (Upload upload) => counts.TryGetValue(upload.Encounter.BossId, out var value) && value > count,
                account.DpsReportUserToken
            ).ConfigureAwait(false);

            if (uploads.Count > 0)
            {
                var successIcon = DiscordEmoteService.GetCheckEmote(_client).ToString();
                var builder = new DpsReportEmbedBuilder();

                builder.WithColor(Color.Green)
                       .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                       .WithTitle(LocalizationGroup.GetFormattedText("DpsReportHistory", "Your last {0} {1} {2} tries", count, successIcon, group.AsText()));

                foreach (var bossGroup in uploads.OrderBy(obj => _dpsReportConnector.GetSortValue(obj.Encounter.BossId))
                                                 .GroupBy(obj => new { obj.Encounter.BossId, obj.Encounter.Boss }))
                {
                    var title = $"{DiscordEmoteService.GetGuildEmote(_client, _dpsReportConnector.GetRaidBossIconId(bossGroup.Key.BossId))} {bossGroup.Key.Boss}";
                    var reports = new StringBuilder();

                    foreach (var upload in bossGroup.OrderByDescending(obj => obj.EncounterTime).Take(count).OrderBy(obj => obj.EncounterTime))
                    {
                        reports.Append(" └ ");
                        reports.Append(Format.Url($"{upload.EncounterTime:dd.MM} - {upload.Encounter.Duration:mm\\:ss} ⧉", upload.Permalink));
                        reports.AppendLine();
                    }

                    builder.AddReportGroup(title, reports.ToString());
                }

                await context.ReplyAsync(embed: builder.Build())
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
    /// Posts the guild raid summary of the raid appointments in a week
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

        var tasks = new Dictionary<long, Task<IEnumerable<Upload>>>();

        foreach (var appointment in appointments)
        {
            tasks.Add(appointment.Id, GetLogsForGuildRaidDay(appointment.Id));
        }

        var bAnyUploads = false;
        var embeds = new List<Embed>();
        var knownGroups = new HashSet<PlayerGroup>();

        // Wait for the tasks in reverse, as more logs are on the first raid appointment
        foreach (var task in tasks.Reverse())
        {
            var uploads = await task.Value.ConfigureAwait(false);

            if (uploads.Any())
            {
                bAnyUploads = true;

                var summaryBuilder = new DpsReportEmbedBuilder();

                summaryBuilder.WithColor(Color.DarkPurple)
                              .WithTitle($"{LocalizationGroup.GetText("DpsReportGuildRaidBossesTitle", "Bosses")} - {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(appointments.First(obj => obj.Id == task.Key).TimeStamp.DayOfWeek)}");

                var groups = await WriteRaidSummary(summaryBuilder, uploads).ConfigureAwait(false);

                foreach (var group in groups)
                {
                    knownGroups.Add(group);
                }

                embeds.Insert(0, summaryBuilder.Build());
            }
        }

        if (bAnyUploads)
        {
            var week = $"{startOfWeek:dd.} - {startOfWeek.AddDays(6):dd.MM.yy}";
            var title = Format.Bold(LocalizationGroup.GetFormattedText("DpsReportGuildRaidSummaryTitle", "Guild Raid {0}", week));

            embeds.Insert(0, BuildStats(knownGroups));

            await context.ReplyAsync(text: title, embeds: embeds.ToArray())
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
    /// <param name="builder">Embed Builder</param>
    /// <param name="uploads">DPS report uploads</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<HashSet<PlayerGroup>> WriteRaidSummary(DpsReportEmbedBuilder builder, IEnumerable<Upload> uploads)
    {
        var knownGroups = new HashSet<PlayerGroup>();

        foreach (var typeGroup in uploads.OrderBy(obj => _dpsReportConnector.GetSortValue(obj.Encounter.BossId))
                                         .ThenBy(obj => obj.EncounterTime)
                                         .GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId).GetReportType()))
        {
            foreach (var reportGroup in typeGroup.GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId)))
            {
                var isFractal = reportGroup.Key.GetReportType() == DpsReportType.Fractal;
                var groupedUploads = GroupUploads(ref knownGroups, reportGroup, true);
                var hasMultipleGroups = groupedUploads.Count() > 1;

                Parallel.ForEach(groupedUploads, groupedUploads =>
                {
                    foreach (var upload in groupedUploads.Value)
                    {
                        groupedUploads.Key.Stats?.AddEncounter(upload);
                    }
                });

                foreach (var groupUploads in groupedUploads)
                {
                    var fieldTitle = new StringBuilder();
                    fieldTitle.Append(reportGroup.Key.AsText());

                    if (hasMultipleGroups)
                    {
                        fieldTitle.AppendLine();
                        fieldTitle.Append(" └ ");
                        fieldTitle.Append(LocalizationGroup.GetFormattedText("DpsReportPlayerGroup", "Group {0}", groupUploads.Key.Id));
                    }

                    var reports = new StringBuilder();

                    foreach (var bossGroup in groupUploads.Value.GroupBy(obj => new { obj.Encounter.BossId, obj.Encounter.Boss }))
                    {
                        // Start a new field, when we don't have enough space for some logs
                        if (reports.Length > 896)
                        {
                            builder.AddReportGroup(fieldTitle.ToString(), reports.ToString());
                            reports = new StringBuilder();
                        }

                        var hasNormalTries = isFractal || bossGroup.Any(obj => !obj.Encounter.IsChallengeMode);
                        var hasChallengeTries = !isFractal && bossGroup.Any(obj => obj.Encounter.IsChallengeMode);

                        foreach (var boss in bossGroup.GroupBy(obj => obj.Encounter.IsChallengeMode).OrderBy(obj => obj.Key))
                        {
                            var title = BuildTitle(boss, true, hasNormalTries, hasChallengeTries);
                            reports.AppendLine(title);

                            // Enrich the logs with the remaining health
                            var hasSuccessTry = boss.Any(obj => obj.Encounter.Success);
                            var fullLogs = await LoadRemainingHealths(boss, (Upload upload) => hasSuccessTry && !upload.Encounter.Success).ConfigureAwait(false);

                            WriteLogs(builder, ref reports, title, fullLogs, true, hasNormalTries, hasChallengeTries);
                        }
                    }

                    builder.AddReportGroup(fieldTitle.ToString(), reports.ToString());
                }
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

        return result.OrderBy(obj => obj.Key.Id);
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
    /// Builds title for the given uploads
    /// </summary>
    /// <param name="uploads">Uploads</param>
    /// <param name="summarize">Whether to summarize the reports</param>
    /// <param name="hasNormalTries">Whether there are normal tries</param>
    /// <param name="hasChallengeTries">Whether there are challenge mode tries</param>
    /// <returns>Title for the given uploads</returns>
    private string BuildTitle(IEnumerable<Upload> uploads, bool summarize, bool hasNormalTries, bool hasChallengeTries)
    {
        var encounter = uploads.First().Encounter;
        var isFractal = _dpsReportConnector.GetReportType(encounter.BossId) == DpsReportType.Fractal;
        var title = new StringBuilder();

        if (!encounter.IsChallengeMode || isFractal || !hasNormalTries)
        {
            title.Append(RetrieveBossIconEmote(encounter.BossId));
            title.Append(' ');
            title.Append(encounter.Boss);
        }

        if (!isFractal && encounter.IsChallengeMode)
        {
            if (!hasNormalTries)
            {
                title.Append(" CM");
            }
            else
            {
                if (!encounter.IsChallengeMode || !hasNormalTries)
                {
                    title.AppendLine();
                }

                title.Append(" └ CM");
            }
        }

        WriteFails(title, uploads, summarize, hasNormalTries, hasChallengeTries);

        return title.ToString();
    }

    /// <summary>
    /// Retrieves the text for the boss icon emoji
    /// </summary>
    /// <param name="bossId">Id of the boss</param>
    /// <returns>Text for the boss icon emoji</returns>
    private string RetrieveBossIconEmote(int bossId)
    {
        return DiscordEmoteService.GetGuildEmote(_client, _dpsReportConnector.GetRaidBossIconId(bossId)).ToString();
    }

    /// <summary>
    /// Writes the fails from the given uploads to the given builder
    /// </summary>
    /// <param name="builder">String builder</param>
    /// <param name="uploads">Uploads</param>
    /// <param name="summarize">Whether to summarize the reports</param>
    /// <param name="hasNormalTries">Whether there are normal tries</param>
    /// <param name="hasChallengeTries">Whether there are challenge mode tries</param>
    private void WriteFails(StringBuilder builder, IEnumerable<Upload> uploads, bool summarize, bool hasNormalTries, bool hasChallengeTries)
    {
        if (summarize)
        {
            var fails = BuildFailsText(uploads);

            if (!string.IsNullOrEmpty(fails))
            {
                builder.AppendLine();

                if (hasNormalTries && hasChallengeTries)
                {
                    builder.Append(' ');
                }

                builder.Append(fails);
            }
        }
    }

    /// <summary>
    /// Returns a text how many tries where made in the given uploads
    /// </summary>
    /// <param name="uploads">The uploads to determine the try count</param>
    /// <returns>Text how many tries were made, or null on first try</returns>
    private string BuildFailsText(IEnumerable<Upload> uploads)
    {
        var fails = uploads.Where(obj => !obj.Encounter.Success);
        var failCount = fails.Count();

        if (failCount > 0 && uploads.Any(obj => obj.Encounter.Success))
        {
            var totalDuration = TimeSpan.FromSeconds(fails.Select(obj => obj.Encounter.Duration.TotalSeconds).Sum());
            var result = new StringBuilder();

            result.Append(" │ ");
            result.Append(FailureIcon);
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
    /// Writes the given logs to the given field string builder
    /// </summary>
    /// <param name="builder">Embed builder</param>
    /// <param name="reports">Field string builder</param>
    /// <param name="title">Title of the current field</param>
    /// <param name="uploads">Uploads</param>
    /// <param name="summarize">Whether to summarize the reports</param>
    /// <param name="hasNormalTries">Whether there are normal tries</param>
    /// <param name="hasChallengeTries">Whether there are challenge mode tries</param>
    private void WriteLogs(DpsReportEmbedBuilder builder, ref StringBuilder reports, string title, List<Tuple<Upload, double?>> uploads, bool summarize, bool hasNormalTries, bool hasChallengeTries)
    {
        // Optionally filter all uploads
        IEnumerable<Tuple<Upload, double?>> filteredUploads = uploads.Count > 0 && summarize && !uploads.Any(obj => obj.Item1.Encounter.Success)
            ? uploads.OrderBy(obj => obj.Item2).Take(3).OrderByDescending(obj => obj.Item2)
            : uploads;

        foreach (var (upload, remainingHealth) in filteredUploads)
        {
            var duration = upload.Encounter.Duration.ToString(@"mm\:ss");
            var percentage = string.Empty;

            if (remainingHealth != null)
            {
                percentage = $" {Math.Floor(remainingHealth.Value)}% -";
            }

            var line = $"{(hasNormalTries && hasChallengeTries ? " " : string.Empty)} └ {(upload.Encounter.Success ? SuccessIcon : FailureIcon)}{percentage} {Format.Url($"{duration} ⧉", upload.Permalink)}";

            // Start a new field, when we would reach the character limit
            if (reports.Length + line.Length > 1024)
            {
                builder.AddReportGroup(title.ToString(), reports.ToString());
                reports = new StringBuilder();

                if (upload.Encounter.IsChallengeMode || !hasNormalTries)
                {
                    reports.Append(RetrieveBossIconEmote(upload.Encounter.BossId));
                    reports.Append(' ');
                    reports.Append(upload.Encounter.Boss);
                    reports.AppendLine();
                }

                reports.AppendLine(title);
            }

            reports.AppendLine(line);
        }
    }

    /// <summary>
    /// Builds an embed of the stats from the given groups
    /// </summary>
    /// <param name="knownGroups">All known groups</param>
    /// <returns>Embed representing the group stats</returns>
    private Embed BuildStats(HashSet<PlayerGroup> knownGroups)
    {
        var dpsIcon = DiscordEmoteService.GetDamageDealerEmote(_client).ToString();
        var builder = new DpsReportEmbedBuilder();

        builder.WithColor(new Color(160, 132, 23))
               .WithTitle(LocalizationGroup.GetText("DpsReportGuildRaidStatsTitle", "Stats"));

        foreach (var groups in knownGroups.OrderBy(obj => obj.Date).GroupBy(obj => obj.Date))
        {
            var hasMultipleGroups = groups.Count() > 1;

            foreach (var group in groups)
            {
                var title = new StringBuilder();
                title.Append(Format.Bold(LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(groups.Key.DayOfWeek)));

                if (hasMultipleGroups)
                {
                    title.Append(" - ");
                    title.Append(LocalizationGroup.GetFormattedText("DpsReportPlayerGroup", "Group {0}", group.Id));
                }

                var content = new StringBuilder();

                content.Append("└ ");
                content.Append(LocalizationGroup.GetFormattedText("DpsReportEncounterStat", "{0} Encounter", group.Stats.SuccessfulEncounters));
                content.AppendLine();

                content.Append("└ ");
                content.Append("Ø ");
                content.Append(group.Stats.AverageDPS.ToString("n0", LocalizationGroup.CultureInfo.NumberFormat));
                content.Append(' ');
                content.Append(dpsIcon);
                content.AppendLine();

                content.Append("└ ");
                content.Append(LocalizationGroup.GetFormattedText("DpsReportFightDurationStat", "{0} in {1} fights", group.Stats.FailedEncounterTotalTime.ToString(group.Stats.FailedEncounterTotalTime.TotalHours > 1.0 ? @"hh\:mm\:ss" : @"mm\:ss"), FailureIcon));
                content.AppendLine();

                content.Append("└ ");
                content.Append(LocalizationGroup.GetFormattedText("DpsReportFightDurationStat", "{0} in {1} fights", group.Stats.SuccessfulEncounterTotalTime.ToString(group.Stats.SuccessfulEncounterTotalTime.TotalHours > 1.0 ? @"hh\:mm\:ss" : @"mm\:ss"), SuccessIcon));
                content.AppendLine();

                content.Append("└ ");
                content.Append(group.Stats.FirstEncounterTime.ToLocalTime().ToString("HH:mm"));
                content.Append(" - ");
                content.Append(group.Stats.LastEncounterTime.ToLocalTime().ToString("HH:mm"));
                content.Append(' ');
                content.Append("\uD83D\uDD57");
                content.AppendLine();

                builder.AddReportGroup(title.ToString(), content.ToString());
            }
        }

        return builder.Build();
    }

    #endregion // Methods
}