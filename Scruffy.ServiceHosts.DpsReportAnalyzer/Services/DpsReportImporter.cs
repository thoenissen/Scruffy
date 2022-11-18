using Microsoft.EntityFrameworkCore;

using MongoDB.Driver;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.ServiceHosts.DpsReportAnalyzer.Data;
using Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Upload;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Services
{
    /// <summary>
    /// Daily report import
    /// </summary>
    public class DpsReportImporter : LocatedAsyncJob
    {
        #region Fields

        /// <summary>
        /// Connector
        /// </summary>
        private readonly DpsReportConnector _connector;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connector">dps.report connector</param>
        public DpsReportImporter(DpsReportConnector connector)
        {
            _connector = connector;
        }

        #endregion // Constructor

        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteOverrideAsync()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var client = new MongoClient(Environment.GetEnvironmentVariable("SCRUFFY_MONGODB_CONNECTION"));

                var reportsCollection = client.GetDatabase(Environment.GetEnvironmentVariable("SCRUFFY_MONGODB_DATABASE"))
                                              .GetCollection<ReportContainer>("reports");

                foreach (var token in await dbFactory.GetRepository<UserRepository>()
                                                     .GetQuery()
                                                     .Where(obj => obj.DpsReportUserToken != null)
                                                     .Select(obj => obj.DpsReportUserToken)
                                                     .ToListAsync()
                                                     .ConfigureAwait(false))
                {
                    var lastReportId = await reportsCollection.Find(obj => obj.UserToken == token)
                                                              .SortByDescending(obj => obj.MetaData.UploadTime)
                                                              .Project(obj => obj.MetaData.Id)
                                                              .FirstOrDefaultAsync()
                                                              .ConfigureAwait(false);

                    var currentPage = 0;
                    var uploads = default(Page);

                    do
                    {
                        var reports = new List<InsertOneModel<ReportContainer>>();

                        currentPage++;
                        uploads = await _connector.GetUploads(token, currentPage)
                                                  .ConfigureAwait(false);

                        foreach (var upload in uploads.Uploads
                                                      .TakeWhile(upload => upload.Id != lastReportId))
                        {
                            try
                            {
                                var report = new ReportContainer
                                             {
                                                 UserToken = uploads.UserToken,
                                                 MetaData = upload,
                                                 Details = await _connector.GetLog(upload.Id)
                                                                           .ConfigureAwait(false)
                                             };

                                reports.Add(new InsertOneModel<ReportContainer>(report));
                            }
                            catch (Exception ex)
                            {
                                LoggingService.AddServiceLogEntry(Scruffy.Data.Enumerations.General.LogEntryLevel.Error, nameof(DpsReportImporter), "Get log data failed.", null, ex);
                            }
                        }

                        try
                        {
                            await reportsCollection.BulkWriteAsync(reports,
                                                                   new BulkWriteOptions
                                                                   {
                                                                       IsOrdered = false
                                                                   })
                                                   .ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            LoggingService.AddServiceLogEntry(Scruffy.Data.Enumerations.General.LogEntryLevel.Error, nameof(DpsReportImporter), "Bulk insert failed.", null, ex);
                        }
                    }
                    while (currentPage < uploads?.Pages);
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}