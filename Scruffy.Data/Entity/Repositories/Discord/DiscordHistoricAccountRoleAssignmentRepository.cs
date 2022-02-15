using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Discord;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Repositories.Discord;

/// <summary>
/// Repository for accessing <see cref="DiscordHistoricAccountRoleAssignmentEntity"/>
/// </summary>
public class DiscordHistoricAccountRoleAssignmentRepository : RepositoryBase<DiscordHistoricAccountRoleAssignmentQueryable, DiscordHistoricAccountRoleAssignmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public DiscordHistoricAccountRoleAssignmentRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Bulk insert
    /// </summary>
    /// <param name="entries">Entries</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> BulkInsert(List<(ulong ServerId, ulong UserId, ulong RoleId)> entries)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #DiscordHistoricAccountRoleAssignments (
                                                                       [Date] datetime2(7) NOT NULL,
                                                                       [ServerId] bigint NOT NULL,
                                                                       [AccountId] bigint NOT NULL,
                                                                       [RoleId] bigint NOT NULL
                                                                   );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var table = new DataTable();
                table.Columns.Add(nameof(DiscordHistoricAccountRoleAssignmentEntity.Date), typeof(DateTime));
                table.Columns.Add(nameof(DiscordHistoricAccountRoleAssignmentEntity.ServerId), typeof(long));
                table.Columns.Add(nameof(DiscordHistoricAccountRoleAssignmentEntity.AccountId), typeof(long));
                table.Columns.Add(nameof(DiscordHistoricAccountRoleAssignmentEntity.RoleId), typeof(long));

                var today = DateTime.Today;

                foreach (var (serverId, userId, roleId) in entries)
                {
                    // Achievement
                    table.Rows.Add(today, serverId, userId, roleId);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#DiscordHistoricAccountRoleAssignments";

                    await bulk.WriteToServerAsync(table)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [DiscordHistoricAccountRoleAssignments] AS [TARGET]
                                                   USING #DiscordHistoricAccountRoleAssignments AS [Source]
                                                      ON [Target].[Date] = [Source].[Date]
                                                     AND [Target].[ServerId] = [Source].[ServerId]
                                                     AND [Target].[AccountId] = [Source].[AccountId]
                                                     AND [Target].[RoleId] = [Source].[RoleId]
                                          WHEN NOT MATCHED THEN
                                               INSERT ( [Date], [ServerId], [AccountId], [RoleId] )
                                               VALUES ( [Source].[Date], [Source].[ServerId], [Source].[AccountId], [Source].[RoleId] )
                                          WHEN NOT MATCHED BY SOURCE
                                           AND [Target].[Date] = @date THEN
                                               DELETE;",
                                            connection);

                sqlCommand.Parameters.AddWithValue("@date", today);

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