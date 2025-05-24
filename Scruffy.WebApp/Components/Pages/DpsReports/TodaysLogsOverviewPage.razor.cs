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
[Authorize(Roles = "Developer")]
public partial class TodaysLogsOverviewPage
{
    #region Fields

    /// <summary>
    /// Reports
    /// </summary>
    private List<DpsReport> _reports = [];

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

    #endregion // Properties

    #region Methods

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
    protected override async Task OnInitializedAsync()
    {
        if (_reports.Count > 0)
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
                    var response = await HttpClient.GetAsync($"https://dps.report/getUploads?userToken={dpsReportToken}&page={page++}")
                                                   .ConfigureAwait(false);

                    if (response.IsSuccessStatusCode == false)
                    {
                        continue;
                    }

                    var content = await response.Content
                                                .ReadAsStringAsync()
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

                    DateTime? today = null;

                    if ((uploads?.Uploads?.Length > 0) == false)
                    {
                        continue;
                    }

                    foreach (var upload in uploads.Uploads)
                    {
                        var uploadTime = DateTimeOffset.FromUnixTimeSeconds(upload.UploadTime ?? 0);

                        today ??= uploadTime.Date;

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

    #endregion // ComponentBase
}