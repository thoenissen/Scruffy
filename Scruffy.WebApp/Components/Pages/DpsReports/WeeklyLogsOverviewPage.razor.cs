using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.DpsReports;
using Scruffy.Services.GuildWars2.DpsReports;
using Scruffy.WebApp.Components.Pages.DpsReports.Data;
using Scruffy.WebApp.Services;

namespace Scruffy.WebApp.Components.Pages.DpsReports;

/// <summary>
/// Weekly logs overview page
/// </summary>
[Authorize(Roles = "Member")]
public partial class WeeklyLogsOverviewPage : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Dictionary of currently loaded logs for quick lookup
    /// </summary>
    private readonly Dictionary<string, DpsReport> _loadedLogs = [];

    /// <summary>
    /// Guild Wars 2 account names of the user
    /// </summary>
    private List<string> _guildWarsAccountNames = [];

    /// <summary>
    /// List of encounters organized by expansion
    /// </summary>
    private List<DpsReportExpansionEntry> _encounters;

    /// <summary>
    /// Week start date
    /// </summary>
    private DateTime _weekStart;

    /// <summary>
    /// Week end date
    /// </summary>
    private DateTime _weekEnd;

    /// <summary>
    /// Flag indicating whether the page is currently loading
    /// </summary>
    private bool _isPageLoading;

    /// <summary>
    /// Completion source for the load reports operation
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Task for loading the reports asynchronously
    /// </summary>
    private Task _loadTask;

    /// <summary>
    /// Current user ID
    /// </summary>
    private int _userId;

    /// <summary>
    /// Selected report for overlay display
    /// </summary>
    private DpsReport _selectedReport;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Authentication state provider
    /// </summary>
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    /// <summary>
    /// Logger
    /// </summary>
    [Inject]
    private ILogger<WeeklyLogsOverviewPage> Logger { get; set; }

    /// <summary>
    /// Processor for detailed DPS reports
    /// </summary>
    [Inject]
    private DpsReportProcessor DpsReportProcessor { get; set; }

    /// <summary>
    /// Importer
    /// </summary>
    [Inject]
    private DpsReportsMetaImporter DpsReportMetaImporter { get; set; }

    /// <summary>
    /// Report generator
    /// </summary>
    [Inject]
    private DpsReportReportGenerator DpsReportReportGenerator { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Calculates the start of the current week (Monday 07:30 UTC)
    /// </summary>
    /// <returns>The start of the current week</returns>
    private DateTime GetWeekStart()
    {
        var now = DateTime.UtcNow;
        var daysUntilMonday = (int)DayOfWeek.Monday - (int)now.DayOfWeek;

        if (daysUntilMonday > 0)
        {
            daysUntilMonday -= 7;
        }

        var monday = now.AddDays(daysUntilMonday).Date;
        var weekStart = monday.AddHours(7).AddMinutes(30);

        if (weekStart > now)
        {
            weekStart = weekStart.AddDays(-7);
        }

        return weekStart;
    }

    /// <summary>
    /// Loads the weekly DPS reports from the database
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task LoadWeeklyReports(CancellationToken token)
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync()
                                                             .ConfigureAwait(false);
            var nameIdentifier = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nameIdentifier) == false
                && int.TryParse(nameIdentifier, out var userId))
            {
                _userId = userId;
                _weekStart = GetWeekStart();
                _weekEnd = _weekStart.AddDays(7);

                using (var repository = RepositoryFactory.CreateInstance())
                {
                    _guildWarsAccountNames = repository.GetRepository<GuildWarsAccountRepository>()
                                                       .GetQuery()
                                                       .Where(account => account.UserId == userId)
                                                       .Select(account => account.Name)
                                                       .ToList();
                }

                token.ThrowIfCancellationRequested();

                await DpsReportMetaImporter.Import(userId).ConfigureAwait(false);

                token.ThrowIfCancellationRequested();

                _encounters = DpsReportReportGenerator.GetEncounters(userId, token, _weekStart, _weekEnd);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading weekly DPS reports.");
        }

        _isPageLoading = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Loads the DPS logs for a specific boss
    /// </summary>
    /// <param name="boss">The boss to load logs for</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task LoadBossLogs(DpsReportBoss boss, CancellationToken token)
    {
        if (boss.IsLoadingLogs)
        {
            return;
        }

        boss.IsLoadingLogs = true;

        try
        {
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();

            using (var repository = RepositoryFactory.CreateInstance())
            {
                var dpsReportRepository = repository.GetRepository<DpsReportRepository>();
                var bossIds= boss.BossIds.Select(id => (long)id).ToList();
                var dpsReports = dpsReportRepository.GetQuery()
                                                    .Where(r => r.UserId == _userId
                                                                && bossIds.Contains(r.BossId)
                                                                && r.EncounterTime >= _weekStart
                                                                && r.EncounterTime < _weekEnd)
                                                    .OrderByDescending(r => r.EncounterTime)
                                                    .ToList();

                boss.Logs = dpsReports.Select(r => new DpsReportBossLogEntry
                                                   {
                                                       Id = r.Id,
                                                       PermaLink = r.PermaLink,
                                                       EncounterTime = r.EncounterTime,
                                                       IsSuccess = r.IsSuccess
                                                   })
                                           .ToList();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading DPS logs for boss ids [{BossIds}].", string.Join(", ", boss.BossIds));
        }
        finally
        {
            boss.IsLoadingLogs = false;

            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Toggles the boss details (logs)
    /// </summary>
    /// <param name="boss">The boss to toggle</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ToggleBossDetails(DpsReportBoss boss)
    {
        if (boss?.IsSuccessful is null)
        {
            return;
        }

        boss.IsExpanded = boss.IsExpanded == false;

        if (boss.IsExpanded && boss.Logs.Count == 0)
        {
            if (_cancellationTokenSource != null)
            {
                await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);

                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = new CancellationTokenSource();

            await LoadBossLogs(boss, _cancellationTokenSource.Token).ConfigureAwait(false);
        }

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens the overlay for a log entry
    /// </summary>
    /// <param name="logEntry">The log entry to display</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OpenLogOverlay(DpsReportBossLogEntry logEntry)
    {
        if (_loadedLogs.TryGetValue(logEntry.Id, out var existingReport))
        {
            _selectedReport = existingReport;

            await InvokeAsync(StateHasChanged).ConfigureAwait(false);

            return;
        }

        var report = new DpsReport
                     {
                         MetaData = new MetaData
                                    {
                                        Id = logEntry.Id,
                                        IsSuccess = logEntry.IsSuccess,
                                        PermaLink = logEntry.PermaLink,
                                        EncounterTime = new DateTimeOffset(logEntry.EncounterTime),
                                        Boss = "Loading...",
                                        Duration = null
                                    },
                         IsLoadingAdditionalData = true
                     };

        _selectedReport = report;
        _loadedLogs[logEntry.Id] = report;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);

        await GetAdditionalDataAsync(report).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets additional data for the report, like DPS, alacrity and quickness
    /// </summary>
    /// <param name="report">Report</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task GetAdditionalDataAsync(DpsReport report)
    {
        try
        {
            report.FullReport = await DpsReportProcessor.Get(report.MetaData.Id).ConfigureAwait(false);

            if (report.FullReport != null)
            {
                report.MetaData.Boss = report.FullReport.FightName;
                report.MetaData.Duration = TimeSpan.FromMilliseconds(report.FullReport.DurationMS);
                report.OverallStatistics = DpsReportReportGenerator.GetOverallStatistics(report.FullReport);
                report.PersonalStatistics = DpsReportReportGenerator.GetPersonalStatistics(report.FullReport, _guildWarsAccountNames);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading additional data for DPS report {ReportId}.", report.MetaData.Id);
        }

        report.IsLoadingAdditionalData = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the CSS class for the boss item based on its status
    /// </summary>
    /// <param name="isSuccessful">Success status</param>
    /// <returns>CSS class name</returns>
    private string GetBossStatusClass(bool? isSuccessful)
    {
        return isSuccessful switch
               {
                   true => "boss-item-success",
                   false => "boss-item-failure",
                   null => "boss-item-unknown"
               };
    }

    /// <summary>
    /// Gets the CSS class for the success indicator based on status
    /// </summary>
    /// <param name="isSuccessful">Success status</param>
    /// <returns>CSS class name</returns>
    private string GetIndicatorClass(bool? isSuccessful)
    {
        return isSuccessful switch
               {
                   true => "indicator-success",
                   false => "indicator-failure",
                   null => "indicator-unknown"
               };
    }

    /// <summary>
    /// Gets the tooltip text based on the success status
    /// </summary>
    /// <param name="isSuccessful">Success status</param>
    /// <returns>Tooltip text</returns>
    private string GetStatusTooltip(bool? isSuccessful)
    {
        return isSuccessful switch
               {
                   true => LocalizationGroup.GetText("Success", "Successfully defeated"),
                   false => LocalizationGroup.GetText("Unsuccessful", "Unsuccessfully attempted"),
                   null => LocalizationGroup.GetText("NotAttempted", "Not attempted")
               };
    }

    /// <summary>
    /// Closes the overlay
    /// </summary>
    private void OnCloseOverlay()
    {
        _selectedReport = null;

        InvokeAsync(StateHasChanged);
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (_isPageLoading)
        {
            return;
        }

        _isPageLoading = true;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        _loadTask = LoadWeeklyReports(_cancellationTokenSource.Token);
        await _loadTask.ConfigureAwait(false);
    }

    #endregion // ComponentBase

    #region IAsyncDisposable

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource != null)
        {
            await _cancellationTokenSource.CancelAsync()
                                          .ConfigureAwait(false);
            _cancellationTokenSource.Dispose();
        }

        if (_loadTask != null)
        {
            await _loadTask.ConfigureAwait(false);
        }
    }

    #endregion // IAsyncDisposable
}