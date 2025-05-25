using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using GW2EIDPSReport.DPSReportJsons;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Services.Core.Extensions;
using Scruffy.WebApp.Components.Pages.DpsReports.Data;
using Scruffy.WebApp.Services;

namespace Scruffy.WebApp.Components.Pages.DpsReports;

/// <summary>
/// Today's reports
/// </summary>
[Authorize(Roles = "Member")]
public sealed partial class TodaysLogsOverviewPage : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Flag indicating whether the page is currently loading
    /// </summary>
    private bool _isPageLoading;

    /// <summary>
    /// Reports
    /// </summary>
    private List<DpsReport> _reports = [];

    /// <summary>
    /// Completion source for the load reports operation
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Task for loading the reports asynchronously
    /// </summary>
    private Task _loadTask;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Authentication state provider
    /// </summary>
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    /// <summary>
    /// Http client
    /// </summary>
    [Inject]
    private HttpClient HttpClient { get; set; }

    /// <summary>
    /// Processor for detailed DPS reports
    /// </summary>
    [Inject]
    private DpsReportProcessor DpsReportProcessor { get; set; }

    /// <summary>
    /// Logger
    /// </summary>
    [Inject]
    private ILogger<TodaysLogsOverviewPage> Logger { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Loads the reports for today from dps.report
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task LoadReports(CancellationToken token)
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync()
                                                             .ConfigureAwait(false);
            var nameIdentifier = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nameIdentifier) == false
                && int.TryParse(nameIdentifier, out var userId))
            {
                using (var repository = RepositoryFactory.CreateInstance())
                {
                    var dpsReportToken = repository.GetRepository<UserRepository>()
                                                   .GetQuery()
                                                   .Where(user => user.Id == userId)
                                                   .Select(user => user.DpsReportUserToken)
                                                   .FirstOrDefault();

                    _reports = [];
                    var page = 1;
                    DPSReportGetUploadsObject uploads = null;

                    do
                    {
                        var response = await HttpClient.GetAsync($"https://dps.report/getUploads?userToken={dpsReportToken}&page={page++}", token)
                                                       .ConfigureAwait(false);

                        if (response.IsSuccessStatusCode == false)
                        {
                            continue;
                        }

                        var content = await response.Content
                                                    .ReadAsStringAsync(token)
                                                    .ConfigureAwait(false);

                        uploads = JsonConvert.DeserializeObject<DPSReportGetUploadsObject>(content,
                                                                                           new JsonSerializerSettings
                                                                                           {
                                                                                               Error = (_, e) =>
                                                                                               {
                                                                                                   // Sometimes 'foundUploads' is a bool and the deserialization to int? fails
                                                                                                   if (e.ErrorContext.Path == "foundUploads")
                                                                                                   {
                                                                                                       e.ErrorContext.Handled = true;
                                                                                                   }
                                                                                               }
                                                                                           });

                        if ((uploads?.Uploads?.Length > 0) == false)
                        {
                            continue;
                        }

                        var today = DateTime.Today;

                        foreach (var upload in uploads.Uploads)
                        {
                            var uploadTime = DateTimeOffset.FromUnixTimeSeconds(upload.UploadTime ?? 0);

                            if (uploadTime < today)
                            {
                                uploads = null;

                                break;
                            }

                            var report = new DpsReport
                                         {
                                             Id = upload.Id,
                                             IsSuccess = upload.Encounter?.Success == true,
                                             PermaLink = upload.Permalink,
                                             EncounterTime = DateTimeOffset.FromUnixTimeSeconds(upload.EncounterTime ?? 0).ToLocalTime(),
                                             Boss = upload.Encounter?.BossName ?? LocalizationGroup.GetText("Loading", "Loading..."),
                                             Duration = TimeSpan.FromSeconds(upload.Encounter?.Duration ?? 0)
                                         };

                            GetAdditionalDataAsync(report).Forget();

                            _reports.Add(report);
                        }
                    }
                    while (uploads?.Uploads?.Length > 0);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading today's DPS reports.");
        }

        _isPageLoading = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets additional data for the report, like DPS, alacrity and quickness
    /// </summary>
    /// <param name="report">Report</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task GetAdditionalDataAsync(DpsReport report)
    {
        var detailedReport = await DpsReportProcessor.Get(report.Id).ConfigureAwait(false);

        var additionalData = new AdditionalData();

        if (detailedReport != null)
        {
            report.Boss = detailedReport.FightName;
            additionalData.Dps = detailedReport.Players?.Sum(player => player.DpsTargets?.Sum(dpsTarget => dpsTarget.Count > 0 ? dpsTarget[0].Dps : 0));
        }

        report.AdditionalData = additionalData;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (_isPageLoading)
        {
            return;
        }

        _isPageLoading = true;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        _loadTask = LoadReports(_cancellationTokenSource.Token);
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