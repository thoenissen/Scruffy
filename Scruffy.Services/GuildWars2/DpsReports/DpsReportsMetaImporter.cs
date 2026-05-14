using System.Net.Http;

using GW2EIDPSReport.DPSReportJsons;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.DpsReports;
using Scruffy.Data.Entity.Tables.GuildWars2.DpsReports;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Import of dps.report meta data
/// </summary>
public class DpsReportsMetaImporter : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Client factory
    /// </summary>
    private readonly IHttpClientFactory _clientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientFactory">Client factory</param>
    /// <param name="localizationService">Localization service</param>
    public DpsReportsMetaImporter(IHttpClientFactory clientFactory, LocalizationService localizationService)
        : base(localizationService)
    {
        _clientFactory = clientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Imports the meta data for the given user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Import(long userId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var userConfiguration = dbFactory.GetRepository<UserDpsReportsConfigurationRepository>()
                                             .GetQuery()
                                             .Where(configuration => configuration.UserId == userId)
                                             .Select(configuration => new
                                                                      {
                                                                          configuration.UserToken,
                                                                          configuration.LastImport
                                                                      })
                                             .FirstOrDefault();

            if (string.IsNullOrEmpty(userConfiguration?.UserToken))
            {
                ScruffyUserMessageCodeException.Throw(ScruffyUserMessageCodeExceptionCode.NoDpsReportTokenConfiguration);
            }

            List<DpsReportEntity> reports = null;

            using (var client = _clientFactory.CreateClient())
            {
                var page = 1;
                DPSReportGetUploadsObject uploads = null;

                do
                {
                    var response = await client.GetAsync($"https://dps.report/getUploads?userToken={userConfiguration.UserToken}&page={page++}&perPage=100")
                                               .ConfigureAwait(false);

                    if (response.IsSuccessStatusCode == false)
                    {
                        ScruffyUserMessageCodeException.Throw(ScruffyUserMessageCodeExceptionCode.DpsReportImportFailed);
                    }

                    var content = await response.Content
                                                .ReadAsStringAsync()
                                                .ConfigureAwait(false);

                    uploads = JsonConvert.DeserializeObject<DPSReportGetUploadsObject>(content,
                                                                                       new JsonSerializerSettings
                                                                                       {
                                                                                           Converters = {
                                                                                                            new IntOrBoolConverter()
                                                                                                        },
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

                    if (userConfiguration.LastImport != null
                        && uploads.Uploads[0].UploadTime != null
                        && DateTimeOffset.FromUnixTimeSeconds(uploads.Uploads[0].UploadTime.Value).ToLocalTime().DateTime < userConfiguration.LastImport.Value)
                    {
                        break;
                    }

                    reports ??= new List<DpsReportEntity>(uploads.TotalUploads ?? 0);

                    foreach (var upload in uploads.Uploads)
                    {
                        reports.Add(new DpsReportEntity
                                    {
                                        UserId = userId,
                                        Id = upload.Id,
                                        PermaLink = upload.Permalink,
                                        UploadTime = DateTimeOffset.FromUnixTimeSeconds(upload.UploadTime ?? 0).ToLocalTime().DateTime,
                                        EncounterTime = DateTimeOffset.FromUnixTimeSeconds(upload.EncounterTime ?? 0).ToLocalTime().DateTime,
                                        BossId = upload.Encounter?.BossId ?? 0,
                                        IsSuccess = upload.Encounter?.Success == true,
                                        Mode = upload.Encounter?.IsLegendaryCm == true
                                                   ? DpsReportMode.LegendaryChallengeMode
                                                   : upload.Encounter?.IsCm == true
                                                         ? DpsReportMode.ChallengeMode
                                                         : DpsReportMode.Normal,
                                        State = DpsReportProcessingState.Pending
                                    });
                    }
                }
                while (uploads?.Uploads?.Length > 0);
            }

            if (reports?.Count > 0)
            {
                await dbFactory.GetRepository<DpsReportRepository>()
                               .BulkInsert(reports)
                               .ConfigureAwait(false);

                dbFactory.GetRepository<UserDpsReportsConfigurationRepository>()
                         .Refresh(configuration => configuration.UserId == userId,
                                  configuration => configuration.LastImport = reports.Select(report => report.UploadTime).OrderByDescending(date => date).First());
            }
        }
    }

    #endregion // Methods
}