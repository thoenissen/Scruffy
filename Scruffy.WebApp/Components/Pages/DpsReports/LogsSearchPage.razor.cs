using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using GW2EIDPSReport.DPSReportJsons;

using GW2EIJSON;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Services.Core.Extensions;
using Scruffy.WebApp.Components.Pages.DpsReports.Data;
using Scruffy.WebApp.Services;

namespace Scruffy.WebApp.Components.Pages.DpsReports;

/// <summary>
/// dps.report search
/// </summary>
[Authorize(Roles = "Member")]
public sealed partial class LogsSearchPage
{
    #region Constants

    /// <summary>
    /// ID for the alacrity buff
    /// </summary>
    private const int AlacrityId = 30328;

    /// <summary>
    /// ID for the quickness buff
    /// </summary>
    private const int QuicknessId = 1187;

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Pagination state for the reports
    /// </summary>
    private readonly PaginationState _paginationState = new()
                                     {
                                         ItemsPerPage = 20
                                     };

    /// <summary>
    /// dps.report user token
    /// </summary>
    private string _dpsReportToken;

    /// <summary>
    /// Index of the current result
    /// </summary>
    private int _currentIndex = -1;

    /// <summary>
    /// Current result
    /// </summary>
    private GridItemsProviderResult<DpsReport> _currentResult;

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
    /// Gets the items for the grid based on the request parameters
    /// </summary>
    /// <param name="request">Request</param>
    /// <returns>Items</returns>
    private async ValueTask<GridItemsProviderResult<DpsReport>> OnGetItems(GridItemsProviderRequest<DpsReport> request)
    {
        if (_currentIndex == request.StartIndex)
        {
            return _currentResult;
        }

        try
        {
            var dpsReportToken = await GetDpsReportToken().ConfigureAwait(false);
            var response = await HttpClient.GetAsync($"https://dps.report/getUploads?userToken={dpsReportToken}&page={(request.StartIndex / 25) + 1}&perPage=25",
                                                     request.CancellationToken)
                                           .ConfigureAwait(false);

            if (response.IsSuccessStatusCode == false)
            {
                return default;
            }

            var content = await response.Content
                                        .ReadAsStringAsync(request.CancellationToken)
                                        .ConfigureAwait(false);

            var uploads = JsonConvert.DeserializeObject<DPSReportGetUploadsObject>(content,
                                                                                   new JsonSerializerSettings
                                                                                   {
                                                                                       Converters = { new IntOrBoolConverter() },
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
                return default;
            }

            var items = uploads.Uploads
                               .Select(upload => new DpsReport
                                                 {
                                                     Id = upload.Id,
                                                     IsSuccess = upload.Encounter?.Success == true,
                                                     PermaLink = upload.Permalink,
                                                     EncounterTime = DateTimeOffset.FromUnixTimeSeconds(upload.EncounterTime ?? 0).ToLocalTime(),
                                                     Boss = upload.Encounter?.BossName ?? LocalizationGroup.GetText("Loading", "Loading..."),
                                                     Duration = TimeSpan.FromSeconds(upload.Encounter?.Duration ?? 0),
                                                     IsLoadingAdditionalData = true,
                                                 })
                               .ToList();

            Task.Run(() =>
                     {
                         foreach (var item in items)
                         {
                             GetAdditionalDataAsync(item).Forget();
                         }
                     })
                .Forget();

            _currentResult = new GridItemsProviderResult<DpsReport>
                             {
                                 Items = items,
                                 TotalItemCount = uploads.TotalUploads ?? items.Count
                             };
            _currentIndex = request.StartIndex;

            return _currentResult;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading today's DPS reports.");
        }

        return default;
    }

    /// <summary>
    /// Get dps.report token of current user
    /// </summary>
    /// <returns>Token</returns>
    private async ValueTask<string> GetDpsReportToken()
    {
        if (_dpsReportToken == null)
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync()
                                                             .ConfigureAwait(false);
            var nameIdentifier = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nameIdentifier) == false
                && int.TryParse(nameIdentifier, out var userId))
            {
                using (var repository = RepositoryFactory.CreateInstance())
                {
                     _dpsReportToken = repository.GetRepository<UserRepository>()
                                                 .GetQuery()
                                                 .Where(user => user.Id == userId)
                                                 .Select(user => user.DpsReportUserToken)
                                                 .FirstOrDefault();
                }
            }
        }

        return _dpsReportToken;
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
            additionalData.Alacrity = GetUptime(detailedReport.Players, AlacrityId);
            additionalData.Quickness = GetUptime(detailedReport.Players, QuicknessId);
        }

        report.IsLoadingAdditionalData = false;
        report.AdditionalData = additionalData;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Calculates the uptime of a specific buff across all players in the report
    /// </summary>
    /// <param name="players">Players</param>
    /// <param name="buffId">Buff ID</param>
    /// <returns>Average Uptime</returns>
    private double? GetUptime(IReadOnlyList<JsonPlayer> players, int buffId)
    {
        if (players == null
            || players.Count == 0)
        {
            return null;
        }

        double weightedUptime = 0;
        long summedActiveTime = 0;

        foreach (var player in players)
        {
            if (player.ActiveTimes == null
                || player.ActiveTimes.Count == 0)
            {
                continue;
            }

            var playerActiveTime = player.ActiveTimes[0];
            summedActiveTime += playerActiveTime;

            if (player.BuffUptimesActive == null
                || player.BuffUptimesActive.Count == 0)
            {
                continue;
            }

            var buffUptime = player.BuffUptimesActive.FirstOrDefault(buf => buf.Id == buffId);

            if (buffUptime?.BuffData == null
                || buffUptime.BuffData.Count == 0)
            {
                continue;
            }

            weightedUptime += buffUptime.BuffData[0].Uptime * playerActiveTime;
        }

        return summedActiveTime > 0
                   ? weightedUptime / summedActiveTime
                   : null;
    }

    /// <summary>
    /// Gets the skill level based on the uptime percentage
    /// </summary>
    /// <param name="uptime">Uptime</param>
    /// <returns>Skill-Level CSS class</returns>
    private string GetSkillLevelFromUptime(double? uptime)
    {
        if (uptime > 80.00D)
        {
            return "skill-level-2";
        }

        if (uptime > 50.00D)
        {
            return "skill-level-1";
        }

        return "skill-level-0";
    }

    #endregion // Methods
}