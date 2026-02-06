using System.Collections;
using System.Globalization;
using System.IO;

using Discord;
using Discord.WebSocket;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Data.Json.DpsReport;
using Scruffy.Data.Json.QuickChart;
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
    private static readonly string[] _dateFormats = ["dd.MM", "dd.MM.yyyy", "MM-dd", "yyyy-MM-dd"];

    /// <summary>
    /// Web application URL
    /// </summary>
    private static readonly string _webbAppUrl = Environment.GetEnvironmentVariable("SCRUFFY_WEBAPP_URL");

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
    /// QuickChart.io connector
    /// </summary>
    private readonly QuickChartConnector _quickChartConnector;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="client">The Discord client</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="dpsReportConnector">DPS-Report connector</param>
    /// <param name="quickChartConnector">QuickChart.io connector</param>
    public LogCommandHandler(LocalizationService localizationService,
                              DiscordSocketClient client,
                              RepositoryFactory repositoryFactory,
                              DpsReportConnector dpsReportConnector,
                              QuickChartConnector quickChartConnector)
        : base(localizationService)
    {
        _client = client;
        _repositoryFactory = repositoryFactory;
        _dpsReportConnector = dpsReportConnector;
        _quickChartConnector = quickChartConnector;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// String to display the success icon
    /// </summary>
    private string SuccessIcon => DiscordEmoteService.GetCheckEmote(_client).ToString();

    /// <summary>
    /// String to display the failure icon
    /// </summary>
    private string FailureIcon => DiscordEmoteService.GetCrossEmote(_client).ToString();

    #endregion // Properties

    #region Public methods

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

        var startDate = ParseStartDate(dayString);
        var endDate = startDate.AddDays(1);

        if (string.IsNullOrEmpty(account?.DpsReportUserToken) == false)
        {
            var uploads = await _dpsReportConnector.GetUploads(account.DpsReportUserToken,
                                                               startDate,
                                                               endDate,
                                                               filter: upload => (type == DpsReportType.All && _dpsReportConnector.GetReportType(upload.Encounter.BossId) != DpsReportType.Other)
                                                                              || type == _dpsReportConnector.GetReportType(upload.Encounter.BossId))
                                                   .ToListAsync()
                                                   .ConfigureAwait(false);

            if (uploads.Count > 0)
            {
                var builder = new DpsReportEmbedBuilder();

                builder.WithColor(Color.Green)
                       .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                       .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", startDate.ToString("d", LocalizationGroup.CultureInfo)));

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
                                        .Select(obj => new
                                                       {
                                                           obj.User.GuildWarsAccounts.FirstOrDefault().Name,
                                                           obj.User.DpsReportUserToken
                                                       })
                                        .FirstOrDefault();

        if (string.IsNullOrEmpty(account?.DpsReportUserToken) == false)
        {
            var counts = new Dictionary<int, int>();

            var uploads = await _dpsReportConnector.GetUploads(account.DpsReportUserToken,
                                                               upload =>
                                                               {
                                                                   if (upload.Encounter.Success && _dpsReportConnector.GetReportGroup(upload.Encounter.BossId) == group)
                                                                   {
                                                                       counts.TryAdd(upload.Encounter.BossId, 0);
                                                                       ++counts[upload.Encounter.BossId];

                                                                       return true;
                                                                   }

                                                                   return false;
                                                               },
                                                               upload => counts.TryGetValue(upload.Encounter.BossId, out var value) && value > count)
                                                   .ToListAsync()
                                                   .ConfigureAwait(false);

            if (uploads.Count > 0)
            {
                var successIcon = DiscordEmoteService.GetCheckEmote(_client).ToString();
                var builder = new DpsReportEmbedBuilder();

                builder.WithColor(Color.Green)
                       .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                       .WithTitle(LocalizationGroup.GetFormattedText("DpsReportHistory", "Your last {0} {1} {2} tries", count, successIcon, group.AsText()));

                async Task AddReportGroup(StringBuilder reports, string title, bool isUseEmptyTitle)
                {
                    if (reports.Length + title.Length + builder.Length >= 5900)
                    {
                        await context.SendMessageAsync(embed: builder.Build())
                                     .ConfigureAwait(false);

                        builder = new DpsReportEmbedBuilder();

                        builder.WithColor(Color.Green)
                               .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                               .WithTitle(LocalizationGroup.GetFormattedText("DpsReportHistory", "Your last {0} {1} {2} tries", count, successIcon, group.AsText()));

                        isUseEmptyTitle = false;
                    }

                    builder.AddReportGroup(isUseEmptyTitle ? "\u200b" : title, reports.ToString(), isUseEmptyTitle == false);
                }

                foreach (var bossGroup in uploads.OrderBy(obj => _dpsReportConnector.GetSortValue(obj.Encounter.BossId))
                                                 .GroupBy(obj => new
                                                                 {
                                                                     obj.Encounter.BossId,
                                                                     obj.Encounter.Boss
                                                                 }))
                {
                    var title = $"{DiscordEmoteService.GetGuildEmote(_client, _dpsReportConnector.GetRaidBossIconId(bossGroup.Key.BossId))} {bossGroup.Key.Boss}";
                    var reports = new StringBuilder();
                    var isUseEmptyTitle = false;

                    foreach (var upload in bossGroup.OrderByDescending(obj => obj.EncounterTime)
                                                    .Take(count)
                                                    .OrderBy(obj => obj.EncounterTime))
                    {
                        var line = $" └ {Format.Url($"{upload.EncounterTime:dd.MM} - {upload.Encounter.Duration:mm\\:ss} ⧉", upload.Permalink)}{Environment.NewLine}";

                        if (line.Length + reports.Length >= 1024)
                        {
                            await AddReportGroup(reports, title, isUseEmptyTitle).ConfigureAwait(false);

                            reports = new StringBuilder();

                            isUseEmptyTitle = true;
                        }

                        reports.Append(line);
                    }

                    await AddReportGroup(reports, title, isUseEmptyTitle).ConfigureAwait(false);
                }

                await context.SendMessageAsync(embed: builder.Build())
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
    /// Posts the last <paramref name="count"/> of <paramref name="group"/> tries as a chart
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="group">Group to search for</param>
    /// <param name="count">Max count logs to search</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostChart(IContextContainer context, DpsReportGroup group, int count)
    {
        var account = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                .GetQuery()
                                .Where(obj => obj.Id == context.User.Id)
                                .Select(obj => new
                                               {
                                                   obj.User.GuildWarsAccounts.FirstOrDefault().Name,
                                                   obj.User.DpsReportUserToken
                                               })
                                .FirstOrDefault();

        if (string.IsNullOrEmpty(account?.DpsReportUserToken) == false)
        {
            var counts = new Dictionary<string, int>();

            var uploads = await _dpsReportConnector.GetUploads(account.DpsReportUserToken,
                                                               upload =>
                                                               {
                                                                   if (upload.Encounter.Success
                                                                    && _dpsReportConnector.GetReportGroup(upload.Encounter.BossId) == group)
                                                                   {
                                                                       if (counts.TryGetValue(upload.FightName, out var fightCount))
                                                                       {
                                                                           if (fightCount < count)
                                                                           {
                                                                               ++counts[upload.FightName];

                                                                               return true;
                                                                           }

                                                                           return false;
                                                                       }

                                                                       counts.Add(upload.FightName, 1);

                                                                       return true;
                                                                   }

                                                                   return false;
                                                               },
                                                               upload => counts.Count > 0 && counts.All(obj => obj.Value == count))
                                                   .ToListAsync()
                                                   .ConfigureAwait(false);

            if (uploads.Count > 0)
            {
                foreach (var bossGroup in uploads.GroupBy(obj => obj.FightName))
                {
                    var builder = new EmbedBuilder().WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                                                    .WithTitle(bossGroup.First().FightName)
                                                    .WithColor(Color.Green)
                                                    .WithImageUrl("attachment://chart.png");

                    var chartConfiguration = new ChartConfigurationData
                                             {
                                                 Type = "line",
                                                 Data = new Data.Json.QuickChart.Data
                                                        {
                                                            DataSets = [
                                                                           new DataSet<int>
                                                                           {
                                                                               BackgroundColor = ["#316ed5"],
                                                                               BorderColor = "#274d85",
                                                                               Data = bossGroup.OrderBy(obj => obj.EncounterTime)
                                                                                               .Select(obj => obj.Encounter.CompDps)
                                                                                               .ToList()
                                                                           }
                                                                       ],
                                                            Labels = bossGroup.OrderBy(obj => obj.EncounterTime)
                                                                              .Select(obj => obj.EncounterTime.ToString("d", LocalizationGroup.CultureInfo))
                                                                              .ToList()
                                                        },
                                                 Options = new OptionsCollection
                                                           {
                                                               Title = new TitleConfiguration
                                                                       {
                                                                           Display = true,
                                                                           FontColor = "white",
                                                                           FontSize = 18,
                                                                           Text = LocalizationGroup.GetText("GroupDPS", "Group DPS")
                                                                       },
                                                               Scales = new ScalesCollection
                                                                        {
                                                                            XAxes = [
                                                                                        new XAxis
                                                                                        {
                                                                                            Ticks = new AxisTicks
                                                                                                    {
                                                                                                        FontColor = "#b3b3b3"
                                                                                                    }
                                                                                        }
                                                                                    ],
                                                                            YAxes = [
                                                                                        new YAxis
                                                                                        {
                                                                                            Ticks = new AxisTicks
                                                                                                    {
                                                                                                        FontColor = "#b3b3b3"
                                                                                                    }
                                                                                        }
                                                                                    ]
                                                                        },
                                                               Plugins = new PluginsCollection
                                                                         {
                                                                             Legend = false
                                                                         }
                                                           }
                                             };

                    var chartStream = await _quickChartConnector.GetChartAsStream(new ChartData
                                                                                  {
                                                                                      Width = 500,
                                                                                      Height = 300,
                                                                                      DevicePixelRatio = 1,
                                                                                      BackgroundColor = "#262626",
                                                                                      Format = "png",
                                                                                      Config = JsonConvert.SerializeObject(chartConfiguration,
                                                                                                                           new JsonSerializerSettings
                                                                                                                           {
                                                                                                                               NullValueHandling = NullValueHandling.Ignore
                                                                                                                           })
                                                                                  })
                                                                .ConfigureAwait(false);

                    await using (chartStream.ConfigureAwait(false))
                    {
                        await context.SendMessageAsync(embed: builder.Build(), attachments: [new FileAttachment(chartStream, "chart.png")])
                                     .ConfigureAwait(false);
                    }
                }
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
    /// Posts the golem logs of the given day
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="dayString">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostGolemLogs(IContextContainer context, string dayString)
    {
        var message = await context.DeferProcessing()
                                   .ConfigureAwait(false);

        var account = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.Id == context.User.Id)
                                        .Select(obj => new
                                                       {
                                                           obj.User.GuildWarsAccounts.FirstOrDefault().Name,
                                                           obj.User.DpsReportUserToken
                                                       })
                                        .FirstOrDefault();

        if (string.IsNullOrEmpty(account?.DpsReportUserToken) == false)
        {
            var startDate = ParseStartDate(dayString);
            var endDate = startDate.AddDays(1);

            var uploads = await _dpsReportConnector.GetUploads(account.DpsReportUserToken,
                                                               startDate,
                                                               endDate,
                                                               upload => _dpsReportConnector.GetReportGroup(upload.Encounter.BossId) == DpsReportGroup.TrainingArea)
                                                   .ToListAsync()
                                                   .ConfigureAwait(false);

            if (uploads.Count > 0)
            {
                var embeds = new List<EmbedBuilder>();

                var embed = new EmbedBuilder().WithColor(Color.Green)
                                              .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                                              .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", startDate.ToString("d", LocalizationGroup.CultureInfo)));

                foreach (var groupBySpec in uploads.GroupBy(upload => upload.Players.FirstOrDefault().Value?.EliteSpecialization))
                {
                    var fieldBuilder = new StringBuilder();

                    foreach (var upload in groupBySpec.OrderByDescending(obj => obj.UploadTime))
                    {
                        var line = $"└{(upload.Encounter.Success ? DiscordEmoteService.GetCheckEmote(context.Client) : DiscordEmoteService.GetCrossEmote(context.Client))} {Format.Url($"{upload.Encounter.Duration:mm\\:ss} - {upload.Encounter.CompDps.ToString("N0", LocalizationGroup.CultureInfo)} DPS ⧉", upload.Permalink)}";

                        if (line.Length + fieldBuilder.Length >= 1024)
                        {
                            if (embed.Length + fieldBuilder.Length >= 5376)
                            {
                                embeds.Add(embed);

                                embed = new EmbedBuilder().WithColor(Color.Green)
                                                          .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                                                          .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", startDate.ToString("d", LocalizationGroup.CultureInfo)));
                            }

                            embed.AddField($"{GuildWars2Helper.GetSpecializationEmote(groupBySpec.Key ?? 0)} {GuildWars2Helper.GetSpecializationName(groupBySpec.Key ?? 0)}", fieldBuilder.ToString());

                            fieldBuilder = new StringBuilder();
                        }

                        fieldBuilder.AppendLine(line);
                    }

                    if (embed.Length + fieldBuilder.Length >= 5376)
                    {
                        embeds.Add(embed);

                        embed = new EmbedBuilder().WithColor(Color.Green)
                                                  .WithAuthor($"{context.User.Username} - {account.Name}", context.User.GetAvatarUrl())
                                                  .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", startDate.ToString("d", LocalizationGroup.CultureInfo)));
                    }

                    embed.AddField($"{GuildWars2Helper.GetSpecializationEmote(groupBySpec.Key ?? 0)} {GuildWars2Helper.GetSpecializationName(groupBySpec.Key ?? 0)}", fieldBuilder.ToString());
                }

                embeds.Add(embed);

                foreach (var builder in embeds)
                {
                    await context.SendMessageAsync(embed: builder.Build())
                        .ConfigureAwait(false);
                }

                if (string.IsNullOrWhiteSpace(_webbAppUrl) == false)
                {
                    var webAppEmbed = new EmbedBuilder().WithColor(Color.Green)
                                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                        .WithColor(Color.Green)
                                                        .WithTimestamp(DateTime.Now)
                                                        .WithDescription(LocalizationGroup.GetFormattedText("WebAppGolemHint", "Would you like more information about a log? Then check out the new [website]({0}).", _webbAppUrl + "/DpsReports/Today"));

                    await context.SendMessageAsync(embed: webAppEmbed.Build())
                                 .ConfigureAwait(false);
                }
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
    /// <param name="dateOfWeek">Day of the raid week</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostGuildRaidSummary(IContextContainer context, string dateOfWeek)
    {
        var startOfWeek = ParseDay(dateOfWeek, out var startOfWeekDay)
                              ? startOfWeekDay.ToDateTime(TimeOnly.MinValue)
                              : DateTime.Today;

        // Go back to monday, the start of the week
        while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
        {
            startOfWeek = startOfWeek.AddDays(-1);
        }

        startOfWeek = startOfWeek.Date;

        var endOfWeek = startOfWeek.AddDays(7);

        // Search for appointments this week
        var appointments = _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.TimeStamp > startOfWeek && obj.TimeStamp < endOfWeek)
                                             .Select(obj => new { obj.Id, obj.TimeStamp })
                                             .ToList();

        // Search for appointments of last week
        if (appointments.Count == 0)
        {
            endOfWeek = startOfWeek;
            startOfWeek = startOfWeek.AddDays(-7);

            appointments = _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.TimeStamp > startOfWeek && obj.TimeStamp < endOfWeek)
                                             .Select(obj => new { obj.Id, obj.TimeStamp })
                                             .ToList();
        }

        var tasks = appointments.ToDictionary(appointment => appointment.Id,
                                              appointment => GetLogsForGuildRaidDay(appointment.Id));

        var areAnyUploadsAvailable = false;
        var reportEmbeds = new List<Embed>();
        var knownGroups = new HashSet<PlayerGroup>();

        // Wait for the tasks in reverse, as more logs are on the first raid appointment
        foreach (var task in tasks.Reverse())
        {
            var uploads = await task.Value.ConfigureAwait(false);

            if (uploads.Any())
            {
                areAnyUploadsAvailable = true;

                var summaryBuilder = new DpsReportEmbedBuilder();

                summaryBuilder.WithColor(Color.DarkPurple)
                              .WithTitle($"{LocalizationGroup.GetText("DpsReportGuildRaidBossesTitle", "Bosses")} - {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(appointments.First(obj => obj.Id == task.Key).TimeStamp.DayOfWeek)}");

                var groups = await WriteRaidSummary(summaryBuilder, uploads).ConfigureAwait(false);

                foreach (var group in groups)
                {
                    knownGroups.Add(group);
                }

                reportEmbeds.Insert(0, summaryBuilder.Build());
            }
        }

        if (areAnyUploadsAvailable)
        {
            var week = $"{startOfWeek:dd.} - {startOfWeek.AddDays(6):dd.MM.yy}";
            var title = Format.Bold(LocalizationGroup.GetFormattedText("DpsReportGuildRaidSummaryTitle", "Guild Raid {0}", week));

            await context.ReplyAsync(text: title, embed: BuildStats(knownGroups))
                         .ConfigureAwait(false);

            foreach (var reportEmbed in reportEmbeds)
            {
                if (reportEmbed.Length >= 6000)
                {
                    var currentEmbed = new EmbedBuilder().WithTitle(reportEmbed.Title)
                                                         .WithColor(reportEmbed.Color ?? Color.Green);

                    foreach (var field in reportEmbed.Fields)
                    {
                        if (field.Name.Length + field.Value.Length + currentEmbed.Length >= 6000)
                        {
                            await context.Channel
                                         .SendMessageAsync(embed: currentEmbed.Build())
                                         .ConfigureAwait(false);

                            currentEmbed = new EmbedBuilder().WithTitle(reportEmbed.Title)
                                                             .WithColor(reportEmbed.Color ?? Color.Green);
                        }

                        currentEmbed.AddField(field.Name, field.Value, field.Inline);
                    }

                    await context.Channel
                                 .SendMessageAsync(embed: currentEmbed.Build())
                                 .ConfigureAwait(false);
                }
                else
                {
                    await context.Channel
                                 .SendMessageAsync(embed: reportEmbed)
                                 .ConfigureAwait(false);
                }
            }
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("NoDpsReportUploads", "No DPS-Reports found!"))
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Export all logs since the given day
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="sinceDayString">Since day</param>
    /// <param name="untilDayString">Until day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ExportLogs(IContextContainer context, string sinceDayString, string untilDayString)
    {
        await context.DeferProcessing()
                     .ConfigureAwait(false);

        var account = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.Id == context.User.Id)
                                        .Select(obj => new { obj.User.GuildWarsAccounts.FirstOrDefault().Name, obj.User.DpsReportUserToken })
                                        .FirstOrDefault();

        if (string.IsNullOrEmpty(account?.DpsReportUserToken) == false)
        {
            var startDate = ParseStartDate(sinceDayString);
            var endDate = ParseEndDate(untilDayString);

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            await using (memoryStream.ConfigureAwait(false))
            {
                await using (writer.ConfigureAwait(false))
                {
                    await foreach (var upload in _dpsReportConnector.GetUploads(account.DpsReportUserToken, startDate, endDate, skipEnhancement: true).ConfigureAwait(false))
                    {
                        await writer.WriteLineAsync(upload.Permalink).ConfigureAwait(false);
                    }

                    await writer.FlushAsync().ConfigureAwait(false);

                    if (memoryStream.Length > 0)
                    {
                        memoryStream.Position = 0;

                        await context.ReplyAsync(attachments: [new FileAttachment(memoryStream, "logs.txt")])
                                     .ConfigureAwait(false);
                    }
                    else
                    {
                        await context.ReplyAsync(LocalizationGroup.GetText("NoDpsReportUploads", "No DPS-Reports found!"))
                                     .ConfigureAwait(false);
                    }
                }
            }
        }
        else
        {
            await context.ReplyAsync(LocalizationGroup.GetText("NoDpsReportToken", "The are no DPS-Report user tokens assigned to your account."))
                         .ConfigureAwait(false);
        }
    }

    #endregion // Public methods

    #region Private methods

    /// <summary>
    /// Parses the day from the given dayString.
    /// </summary>
    /// <param name="dayString">Day to parse</param>
    /// <param name="day">Date</param>
    /// <returns>Parsed day</returns>
    private bool ParseDay(string dayString, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(dayString) == false)
        {
            return DateOnly.TryParseExact(dayString, _dateFormats, null, DateTimeStyles.None, out day);
        }

        day = default;

        return false;
    }

    /// <summary>
    /// Parses the day date time offset from the given dayString as the first second of the day.
    /// </summary>
    /// <param name="dayString">Day to parse</param>
    /// <returns>Parsed date time offset</returns>
    private DateTimeOffset ParseStartDate(string dayString)
    {
        if (ParseDay(dayString, out var day) == false)
        {
            day = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        return new DateTimeOffset(day.Year, day.Month, day.Day, 0, 0, 0, DateTimeOffset.Now.Offset);
    }

    /// <summary>
    /// Parses the day date time offset from the given dayString as the last second of the day.
    /// </summary>
    /// <param name="dayString">Day to parse</param>
    /// <returns>Parsed date time offset</returns>
    private DateTimeOffset? ParseEndDate(string dayString)
    {
        if (string.IsNullOrEmpty(dayString) == false)
        {
            if (ParseDay(dayString, out var day) == false)
            {
                day = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            return new DateTimeOffset(day.Year, day.Month, day.Day, 23, 59, 59, DateTimeOffset.Now.Offset);
        }

        return null;
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
                                .GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId).GetReportType())
                                .ToList();

        addSubTitles &= typeGroups.Count > 1;

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
                    if (reports.Length > 768)
                    {
                        builder.AddReportGroup(reportGroup.Key, reports.ToString());
                        reports = new StringBuilder();
                    }

                    var hasNormalTries = isFractal
                                      || bossGroup.Any(obj => obj.Encounter.IsChallengeMode == false);
                    var hasChallengeTries = isFractal == false
                                         && bossGroup.Any(obj => obj.Encounter.IsChallengeMode);

                    foreach (var boss in bossGroup.GroupBy(obj => obj.Encounter.IsChallengeMode)
                                                  .OrderBy(obj => obj.Key)
                                                  .Select(obj => obj.ToList()))
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
                            var hasSuccessTry = summarize
                                             && groupUploads.Value.Any(obj => obj.Encounter.Success);

                            var fullLogs = await LoadRemainingHealths(groupUploads.Value, upload => hasSuccessTry && upload.Encounter.Success == false).ConfigureAwait(false);

                            WriteLogs(builder, ref reports, reportGroup.Key.AsText(), title, fullLogs, summarize, hasNormalTries, hasChallengeTries);
                        }
                    }
                }

                builder.AddReportGroup(reportGroup.Key, reports.ToString());
            }
        }
    }

    /// <summary>
    /// Retrieves DPS reports for the given appointment
    /// </summary>
    /// <param name="appointmentId">ID of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task<List<Upload>> GetLogsForGuildRaidDay(long appointmentId)
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

        if (appointment != null)
        {
            var startDate = new DateTimeOffset(appointment.StartTime, DateTimeOffset.Now.Offset).AddMinutes(-15);
            var endDate = startDate.AddHours(3).AddMinutes(30);

            var result = new HashSet<Upload>();

            foreach (var token in appointment.Tokens)
            {
                await foreach (var upload in _dpsReportConnector.GetUploads(token, startDate, endDate,  filter: upload => _dpsReportConnector.GetReportType(upload.Encounter.BossId) == DpsReportType.Raid).ConfigureAwait(false))
                {
                    result.Add(upload);
                }
            }

            return result.ToList();
        }

        return [];
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
        var hasMultipleGroups = false;

        foreach (var typeGroup in uploads.OrderBy(obj => _dpsReportConnector.GetSortValue(obj.Encounter.BossId))
                                         .ThenBy(obj => obj.EncounterTime)
                                         .GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId).GetReportType()))
        {
            foreach (var reportGroup in typeGroup.GroupBy(obj => _dpsReportConnector.GetReportGroup(obj.Encounter.BossId)))
            {
                var isFractal = reportGroup.Key.GetReportType() == DpsReportType.Fractal;
                var groupedUploads = GroupUploads(ref knownGroups, reportGroup, true).ToList();

                if (hasMultipleGroups == false)
                {
                    hasMultipleGroups = groupedUploads.Count > 1;
                }

                Parallel.ForEach(groupedUploads,
                                 uploadGroup =>
                                 {
                                     foreach (var upload in uploadGroup.Value)
                                     {
                                         uploadGroup.Key.Stats?.AddEncounter(upload);
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
                            builder.AddReportGroup(fieldTitle.ToString(), reports.ToString(), true);
                            reports = new StringBuilder();
                        }

                        var hasNormalTries = isFractal
                                          || bossGroup.Any(obj => obj.Encounter.IsChallengeMode == false);
                        var hasChallengeTries = isFractal == false
                                             && bossGroup.Any(obj => obj.Encounter.IsChallengeMode);

                        foreach (var boss in bossGroup.GroupBy(obj => obj.Encounter.IsChallengeMode)
                                                      .OrderBy(obj => obj.Key)
                                                      .Select(obj => obj.ToList()))
                        {
                            var title = BuildTitle(boss, true, hasNormalTries, hasChallengeTries);
                            reports.AppendLine(title);

                            // Enrich the logs with the remaining health
                            var hasSuccessTry = boss.Any(obj => obj.Encounter.Success);
                            var fullLogs = await LoadRemainingHealths(boss, upload => hasSuccessTry && upload.Encounter.Success == false).ConfigureAwait(false);

                            WriteLogs(builder, ref reports, fieldTitle.ToString(), title, fullLogs, true, hasNormalTries, hasChallengeTries);
                        }
                    }

                    builder.AddReportGroup(fieldTitle.ToString(), reports.ToString(), true);
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

            if (result.TryGetValue(group, out var uploadsOfGroup) == false)
            {
                uploadsOfGroup = [];

                result.Add(group, uploadsOfGroup);
            }

            uploadsOfGroup.Add(upload);
        }

        return result.OrderBy(obj => obj.Key.Id);
    }

    /// <summary>
    /// Loads remaining healths in parallel for given uploads
    /// </summary>
    /// <param name="uploads">The uploads to get the logs for</param>
    /// <param name="shouldSkip">A function to determine whether a upload should be skipped</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<List<Tuple<Upload, double?>>> LoadRemainingHealths(List<Upload> uploads, Func<Upload, bool> shouldSkip)
    {
        var logs = uploads.Where(upload => shouldSkip(upload) == false
                                           && upload.Encounter.Success == false
                                           && upload.Encounter.JsonAvailable)
                          .ToDictionary(upload => upload.Id, upload => _dpsReportConnector.GetLog(upload.Id));

        var result = new List<Tuple<Upload, double?>>();

        foreach (var upload in uploads)
        {
            if (shouldSkip(upload) == false)
            {
                if (upload.Encounter.Success == false
                 && upload.Encounter.JsonAvailable)
                {
                    var log = await logs[upload.Id].ConfigureAwait(false);

                    result.Add(new Tuple<Upload, double?>(upload, log?.RemainingTotalHealth));
                }
                else
                {
                    result.Add(new Tuple<Upload, double?>(upload, null));
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
    private string BuildTitle(List<Upload> uploads, bool summarize, bool hasNormalTries, bool hasChallengeTries)
    {
        var encounter = uploads.First().Encounter;
        var isFractal = _dpsReportConnector.GetReportType(encounter.BossId) == DpsReportType.Fractal;
        var title = new StringBuilder();

        if (encounter.IsChallengeMode == false
         || isFractal
         || hasNormalTries == false)
        {
            title.Append(RetrieveBossIconEmote(encounter.BossId));
            title.Append(' ');
            title.Append(encounter.Boss);
        }

        if (isFractal == false
         && encounter.IsChallengeMode)
        {
            if (hasNormalTries == false)
            {
                title.Append(" CM");
            }
            else
            {
                if (encounter.IsChallengeMode == false)
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
    private void WriteFails(StringBuilder builder, List<Upload> uploads, bool summarize, bool hasNormalTries, bool hasChallengeTries)
    {
        if (summarize)
        {
            var fails = BuildFailsText(uploads);

            if (string.IsNullOrEmpty(fails) == false)
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
    private string BuildFailsText(List<Upload> uploads)
    {
        var fails = uploads.Where(obj => obj.Encounter.Success == false)
                           .ToList();

        var failCount = fails.Count;

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
    /// <param name="fieldTitle">Title of the current field</param>
    /// <param name="title">Title of the current boss</param>
    /// <param name="uploads">Uploads</param>
    /// <param name="summarize">Whether to summarize the reports</param>
    /// <param name="hasNormalTries">Whether there are normal tries</param>
    /// <param name="hasChallengeTries">Whether there are challenge mode tries</param>
    private void WriteLogs(DpsReportEmbedBuilder builder, ref StringBuilder reports, string fieldTitle, string title, List<Tuple<Upload, double?>> uploads, bool summarize, bool hasNormalTries, bool hasChallengeTries)
    {
        // Optionally filter all uploads
        var filteredUploads = uploads.Count > 0
                              && summarize
                              && uploads.Any(obj => obj.Item1.Encounter.Success) == false
                                     ? uploads.OrderBy(obj => obj.Item2).Take(3)
                                              .OrderByDescending(obj => obj.Item2)
                                              .ToList()
                                     : uploads;

        foreach (var (upload, remainingHealth) in filteredUploads)
        {
            var duration = upload.Encounter.Duration.ToString(@"mm\:ss");
            var percentage = string.Empty;

            if (remainingHealth != null)
            {
                percentage = $" {Math.Round(remainingHealth.Value)}% -";
            }

            var line = $"{(hasNormalTries && hasChallengeTries ? " " : string.Empty)} └ {(upload.Encounter.Success ? SuccessIcon : FailureIcon)}{percentage} {Format.Url($"{duration} ⧉", upload.Permalink)}";

            // Start a new field, when we would reach the character limit
            if (reports.Length + line.Length > 1024)
            {
                builder.AddReportGroup(fieldTitle, reports.ToString(), true);
                reports = new StringBuilder();

                if (upload.Encounter.IsChallengeMode || hasNormalTries == false)
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

                builder.AddReportGroup(title.ToString(), content.ToString(), true);
            }
        }

        return builder.Build();
    }

    #endregion // Private methods
}