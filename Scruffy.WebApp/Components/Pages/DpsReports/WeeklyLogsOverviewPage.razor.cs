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

using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Services.GuildWars2.DpsReports;
using Scruffy.WebApp.Components.Services.DpsReports;
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

    /// <summary>
    /// Report visualizer
    /// </summary>
    [Inject]
    private DpsReportVisualizer DpsReportVisualizer { get; set; }

    #endregion // Properties

    #region Methods

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
                && int.TryParse(nameIdentifier, out _userId))
            {
                var importTask = DpsReportMetaImporter.Import(_userId);

                (_weekStart, _weekEnd) = DpsReportReportGenerator.GetLastRaidWeek(_userId);
                _guildWarsAccountNames = DpsReportReportGenerator.GetGuildWarsAccountNames(_userId);

                token.ThrowIfCancellationRequested();

                await importTask.ConfigureAwait(false);

                token.ThrowIfCancellationRequested();

                _encounters = DpsReportReportGenerator.GetWeeklyEncounters(_userId, token, _weekStart, _weekEnd);
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

            boss.Logs = DpsReportReportGenerator.GetBossLogs(_userId, boss.BossIds.Select(id => (long)id).ToList(), _weekStart, _weekEnd);
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

        _loadedLogs[logEntry.Id] = _selectedReport = new DpsReport(logEntry);

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);

        await GetAdditionalDataAsync(_selectedReport).ConfigureAwait(false);
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
                DpsReportReportGenerator.FillReportStatistics(report, _guildWarsAccountNames);
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

        if (_cancellationTokenSource != null)
        {
            await _cancellationTokenSource.CancelAsync()
                                          .ConfigureAwait(false);
            _cancellationTokenSource.Dispose();
        }
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