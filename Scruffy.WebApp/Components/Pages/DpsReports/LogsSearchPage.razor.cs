using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using GW2EIDPSReport.DPSReportJsons;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.GuildWars2.DpsReports;
using Scruffy.WebApp.Components.Services.DpsReports;
using Scruffy.WebApp.Services;

namespace Scruffy.WebApp.Components.Pages.DpsReports;

/// <summary>
/// dps.report search
/// </summary>
[Authorize(Roles = "Member")]
public sealed partial class LogsSearchPage : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Pagination state for the reports
    /// </summary>
    private readonly PaginationState _paginationState = new()
                                                        {
                                                            ItemsPerPage = 20
                                                        };

    /// <summary>
    /// Flag indicating whether user data has been loaded
    /// </summary>
    private bool _isUserDataLoaded;

    /// <summary>
    /// dps.report user token
    /// </summary>
    private string _dpsReportToken;

    /// <summary>
    /// Guild Wars 2 account names of the user
    /// </summary>
    private List<string> _guildWarsAccountNames = [];

    /// <summary>
    /// Index of the current result
    /// </summary>
    private int _currentIndex = -1;

    /// <summary>
    /// Current results
    /// </summary>
    private Dictionary<string, DpsReport> _currentItems;

    /// <summary>
    /// Current result
    /// </summary>
    private GridItemsProviderResult<DpsReport> _currentResult;

    /// <summary>
    /// Selected report for overlay display
    /// </summary>
    private DpsReport _selectedReport;

    /// <summary>
    /// Grid container reference for JavaScript interop
    /// </summary>
    private ElementReference _gridContainer;

    /// <summary>
    /// JS module for JavaScript interop
    /// </summary>
    private IJSObjectReference _module;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// JS-Runtime
    /// </summary>
    [Inject]
    private IJSRuntime JsRuntime { get; set; }

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
            await LoadUserDataAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(_dpsReportToken))
            {
                _currentResult = new GridItemsProviderResult<DpsReport>
                                 {
                                     Items = [],
                                     TotalItemCount = 0
                                 };

                return _currentResult;
            }

            var response = await HttpClient.GetAsync($"https://dps.report/getUploads?userToken={_dpsReportToken}&page={(request.StartIndex / 25) + 1}&perPage=25",
                                                     request.CancellationToken)
                                           .ConfigureAwait(false);

            if (response.IsSuccessStatusCode == false)
            {
                _currentResult = new GridItemsProviderResult<DpsReport>
                                 {
                                     Items = [],
                                     TotalItemCount = _currentResult.TotalItemCount
                                 };

                return _currentResult;
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
                _currentResult = new GridItemsProviderResult<DpsReport>
                                 {
                                     Items = [],
                                     TotalItemCount = _currentResult.TotalItemCount
                                 };

                return _currentResult;
            }

            _currentItems = uploads.Uploads
                                   .Select(upload => new DpsReport
                                                     {
                                                         MetaData = new MetaData
                                                                    {
                                                                        Id = upload.Id,
                                                                        IsSuccess = upload.Encounter?.Success == true,
                                                                        PermaLink = upload.Permalink,
                                                                        EncounterTime = DateTimeOffset.FromUnixTimeSeconds(upload.EncounterTime ?? 0).ToLocalTime(),
                                                                        Boss = upload.Encounter?.BossName ?? LocalizationGroup.GetText("Loading", "Loading..."),
                                                                        Duration = TimeSpan.FromSeconds(upload.Encounter?.Duration ?? 0),
                                                                    },
                                                         IsLoadingAdditionalData = true,
                                                     })
                                   .ToDictionary(report => report.MetaData.Id);

            Task.Run(() =>
                     {
                         foreach (var item in _currentItems)
                         {
                             GetAdditionalDataAsync(item.Value).Forget();
                         }
                     })
                .Forget();

            _currentResult = new GridItemsProviderResult<DpsReport>
                             {
                                 Items = _currentItems.Values,
                                 TotalItemCount = uploads.TotalUploads ?? _currentItems.Count
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

        return new GridItemsProviderResult<DpsReport>
               {
                   Items = []
               };
    }

    /// <summary>
    /// Load user data including DPS report token and Guild Wars 2 accounts
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    private async ValueTask LoadUserDataAsync()
    {
        if (_isUserDataLoaded)
        {
            return;
        }

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync()
                                                         .ConfigureAwait(false);
        var nameIdentifier = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(nameIdentifier) == false
            && int.TryParse(nameIdentifier, out var userId))
        {
            using (var repository = RepositoryFactory.CreateInstance())
            {
                var userRepository = repository.GetRepository<UserRepository>();
                var user = userRepository.GetQuery()
                                         .FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    _dpsReportToken = user.DpsReportUserToken;
                }
            }

            _guildWarsAccountNames = DpsReportReportGenerator.GetGuildWarsAccountNames(userId);
        }

        _isUserDataLoaded = true;
    }

    /// <summary>
    /// Gets additional data for the report, like DPS, alacrity and quickness
    /// </summary>
    /// <param name="report">Report</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task GetAdditionalDataAsync(DpsReport report)
    {
        report.FullReport = await DpsReportProcessor.Get(report.MetaData.Id).ConfigureAwait(false);

        if (report.FullReport != null)
        {
            DpsReportReportGenerator.FillReportStatistics(report, _guildWarsAccountNames);
        }

        report.IsLoadingAdditionalData = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Called from JavaScript when a row is clicked
    /// </summary>
    /// <param name="reportId">ID of the report</param>
    [JSInvokable]
    public void SelectReportFromJs(string reportId)
    {
        if (_currentItems.TryGetValue(reportId, out var report))
        {
            _selectedReport = report;

            InvokeAsync(StateHasChanged);
        }
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
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender)
                  .ConfigureAwait(true);

        if (firstRender)
        {
            try
            {
                _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/DpsReports/LogsSearchPage.razor.js")
                                         .ConfigureAwait(true);

                await _module.InvokeVoidAsync("setupRowClickHandlers", _gridContainer, DotNetObjectReference.Create(this))
                             .ConfigureAwait(true);
            }
            catch
            {
            }
        }
    }

    #endregion // ComponentBase

    #region IAsyncDisposable

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }

    #endregion // IAsyncDisposable
}