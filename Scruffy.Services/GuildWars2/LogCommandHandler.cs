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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Logs(IContextContainer context, DpsReportType type, string dayString)
    {
        var pairs = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.Id == context.User.Id)
                                            .Select(obj => new { obj.User.GuildWarsAccounts.FirstOrDefault().Name, obj.User.DpsReportUserToken })
                                            .ToList();

        var tokens = pairs.Select(obj => obj.DpsReportUserToken).Distinct().ToList();
        var userName = pairs.Select(obj => obj.Name).Distinct().FirstOrDefault();

        if (string.IsNullOrWhiteSpace(dayString)
            || DateOnly.TryParseExact(dayString, "yyyy-MM-dd", null, DateTimeStyles.None, out var day) == false)
        {
            day = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        if (tokens.Count > 0)
        {
            var uploads = new List<Upload>();

            foreach (var token in tokens)
            {
                uploads.AddRange(await _dpsReportConnector.GetUploads(token,
                    filter: (Upload upload) =>
                    {
                        var encounterDay = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(upload.EncounterTime).UtcDateTime);
                        return encounterDay == day && (type == DpsReportType.All || type == _dpsReportConnector.GetReportType(upload.Encounter.BossId));
                    },
                    shouldAbort: (Upload upload) =>
                    {
                        var uploadDay = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(upload.UploadTime).UtcDateTime);
                        return uploadDay < day;
                    }
                ).ConfigureAwait(false));
            }

            if (uploads.Count > 0)
            {
                var embedBuilder = new DpsReportEmbedBuilder();

                embedBuilder.WithColor(Color.Green)
                            .WithAuthor($"{context.User.Username} - {userName}", context.User.GetAvatarUrl())
                            .WithTitle(LocalizationGroup.GetFormattedText("DpsReportTitle", "Your reports from {0}", day.ToString("d", LocalizationGroup.CultureInfo)));

                await AddReports(embedBuilder, uploads, type == DpsReportType.All).ConfigureAwait(false);

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
    /// <param name="addSubTitles">Whether sub titles should be added</param>
    /// <returns>A Task representing the async operation</returns>
    private async Task AddReports(DpsReportEmbedBuilder embedBuilder, List<Upload> uploads, bool addSubTitles)
    {
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
                    var hasChallengeTries = !isFractal && bossGroup.Any(obj => obj.Encounter.IsChallengeMode);

                    foreach (var boss in bossGroup.GroupBy(obj => obj.Encounter.IsChallengeMode).OrderBy(obj => obj.Key))
                    {
                        if (reports.Length > 900)
                        {
                            embedBuilder.AddReportGroup(group.Key, reports.ToString());
                            reports = new StringBuilder();
                        }

                        var tries = boss.Count();
                        var title = isFractal || !boss.Key
                            ? $"{DiscordEmoteService.GetGuildEmote(_client, _dpsReportConnector.GetRaidBossIconId(bossGroup.Key.BossId))} {bossGroup.Key.Boss} ({(tries > 1 ? $"{tries} tries" : "First Try")})"
                            : $" └ CM ({(tries > 1 ? $"{tries} tries" : "First Try")})";

                        reports.AppendLine(title);

                        foreach (var upload in boss)
                        {
                            var duration = string.Empty;
                            var result = upload.Encounter.Success ? "Success!" : "Failure";

                            if (upload.Encounter.JsonAvailable)
                            {
                                var log = await _dpsReportConnector.GetLog(upload.Id).ConfigureAwait(false);
                                duration = $"{log.Duration?.ToString(@"mm\:ss")} - ";

                                if (!upload.Encounter.Success)
                                {
                                    var remainingHealth = log.RemainingTotalHealth;

                                    if (remainingHealth != null)
                                    {
                                        result = $"{Math.Floor(remainingHealth.Value)}%";
                                    }
                                }
                            }

                            var line = $"{(hasChallengeTries ? " " : String.Empty)} └ {Format.Url($"{duration}{result}", upload.Permalink)}";

                            if (reports.Length + line.Length > 1000)
                            {
                                embedBuilder.AddReportGroup(group.Key, reports.ToString());
                                reports = new StringBuilder();
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

    #endregion // Methods
}