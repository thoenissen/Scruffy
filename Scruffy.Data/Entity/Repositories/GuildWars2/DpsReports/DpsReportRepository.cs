using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.DpsReports;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.DpsReports;
using Scruffy.Data.Enumerations.GuildWars2;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.DpsReports;

/// <summary>
/// Repository for accessing <see cref="DpsReportEntity"/>
/// </summary>
public class DpsReportRepository : RepositoryBase<DpsReportQueryable, DpsReportEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public DpsReportRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Inserts a list of <see cref="DpsReportEntity"/> into the database
    /// </summary>
    /// <param name="reports">Reports</param>
    /// <returns>Is the operation completed successfully?</returns>
    public async Task<bool> BulkInsert(List<DpsReportEntity> reports)
    {
        var success = false;

        LastError = null;

        try
        {
            var connection = new SqlConnection(GetDbContext().ConnectionString);

            await using (connection.ConfigureAwait(false))
            {
                await connection.OpenAsync()
                                .ConfigureAwait(false);

                var sqlCommand = new SqlCommand(@"CREATE TABLE #DpsReports (
                                                    [UserId] bigint NOT NULL,
                                                    [Id] nvarchar(64) NOT NULL,
                                                    [PermaLink] nvarchar(64) NULL,
                                                    [UploadTime] datetime2 NOT NULL,
                                                    [EncounterTime] datetime2 NOT NULL,
                                                    [BossId] bigint NOT NULL,
                                                    [IsSuccess] bit NOT NULL,
                                                    [Mode] int NOT NULL,
                                                    [State] int NOT NULL);",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var table = new DataTable();
                table.Columns.Add(nameof(DpsReportEntity.UserId), typeof(long));
                table.Columns.Add(nameof(DpsReportEntity.Id), typeof(string));
                table.Columns.Add(nameof(DpsReportEntity.PermaLink), typeof(string));
                table.Columns.Add(nameof(DpsReportEntity.UploadTime), typeof(DateTime));
                table.Columns.Add(nameof(DpsReportEntity.EncounterTime), typeof(DateTime));
                table.Columns.Add(nameof(DpsReportEntity.BossId), typeof(long));
                table.Columns.Add(nameof(DpsReportEntity.IsSuccess), typeof(bool));
                table.Columns.Add(nameof(DpsReportEntity.Mode), typeof(DpsReportMode));
                table.Columns.Add(nameof(DpsReportEntity.State), typeof(DpsReportProcessingState));

                foreach (var report in reports)
                {
                    table.Rows.Add(report.UserId, report.Id, report.PermaLink, report.UploadTime, report.EncounterTime, report.BossId, report.IsSuccess, report.Mode, report.State);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#DpsReports";

                    await bulk.WriteToServerAsync(table)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [DpsReports] AS [TARGET]
                                                   USING ( SELECT DISTINCT [UserId], [Id], [PermaLink], [UploadTime], [EncounterTime], [BossId], [IsSuccess], [Mode], [State] FROM  #DpsReports ) AS [Source]
                                                      ON [Target].[UserId] = [Source].[UserId]
                                                     AND [Target].[Id] = [Source].[Id]
                                          WHEN NOT MATCHED THEN
                                               INSERT ( [UserId], [Id], [PermaLink], [UploadTime], [EncounterTime], [BossId], [IsSuccess], [Mode], [State] )
                                               VALUES ( [Source].[UserId], [Source].[Id], [Source].[PermaLink], [Source].[UploadTime], [Source].[EncounterTime], [Source].[BossId], [Source].[IsSuccess], [Source].[Mode], [Source].[State] );",
                                            connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }

            success = true;
        }
        catch (Exception ex)
        {
            LastError = ex;
        }

        return success;
    }

    #endregion // Methods
}