using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Statistics;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Statistics;
using Scruffy.Data.Services.Statistics;

namespace Scruffy.Data.Entity.Repositories.Statistics
{
    /// <summary>
    /// Repository for accessing <see cref="DiscordMessageEntity"/>
    /// </summary>
    public class DiscordMessageRepository : RepositoryBase<DiscordMessageQueryable, DiscordMessageEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public DiscordMessageRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Bulk insert
        /// </summary>
        /// <param name="entries">Entries</param>
        /// <param name="isBatchCommitted">Batch committed import</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> BulkInsert(List<DiscordMessageBulkInsertData> entries, bool isBatchCommitted)
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

                    var sqlCommand = new SqlCommand(@"CREATE TABLE #DiscordMessages (
                                                                       [DiscordServerId] decimal(20,0) NOT NULL,
                                                                       [DiscordChannelId] decimal(20,0) NOT NULL,
                                                                       [DiscordMessageId] decimal(20,0) NOT NULL,
                                                                       [DiscordAccountId] decimal(20,0) NOT NULL,
                                                                       [TimeStamp] datetime2 NOT NULL,
                                                                   )",
                                                                 connection);

                    await using (sqlCommand.ConfigureAwait(false))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    var dataTable = new DataTable();
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.DiscordServerId), typeof(ulong));
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.DiscordChannelId), typeof(ulong));
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.DiscordMessageId), typeof(ulong));
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.DiscordAccountId), typeof(ulong));
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.TimeStamp), typeof(DateTime));

                    foreach (var entry in entries)
                    {
                        dataTable.Rows.Add(entry.ServerId, entry.ChannelId, entry.MessageId, entry.UserId, entry.TimeStamp);
                    }

                    using (var bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = "#DiscordMessages";

                        await bulk.WriteToServerAsync(dataTable)
                                  .ConfigureAwait(false);
                    }

                    if (isBatchCommitted)
                    {
                        sqlCommand = new SqlCommand(@"MERGE INTO [DiscordMessages] AS [TARGET]
                                                                           USING #DiscordMessages AS [Source]
                                                                              ON [Target].[DiscordServerId] = [Source].[DiscordServerId]
                                                                             AND  [Target].[DiscordChannelId] = [Source].[DiscordChannelId]
                                                                             AND  [Target].[DiscordMessageId] = [Source].[DiscordMessageId]
                                                               WHEN MATCHED THEN
                                                                          UPDATE 
                                                                            SET [Target].[IsBatchCommitted] = 1
                                                               WHEN NOT MATCHED THEN
                                                                          INSERT ( [DiscordServerId], [DiscordChannelId], [DiscordMessageId], [DiscordAccountId], [TimeStamp], [IsBatchCommitted])
                                                                          VALUES ( [Source].[DiscordServerId], [Source].[DiscordChannelId], [Source].[DiscordMessageId], [Source].[DiscordAccountId], [Source].[TimeStamp], 1); ",
                                                                    connection);

                        await using (sqlCommand.ConfigureAwait(false))
                        {
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        sqlCommand = new SqlCommand(@"MERGE INTO [DiscordMessages] AS [TARGET]
                                                                           USING #DiscordMessages AS [Source]
                                                                              ON [Target].[ServerId] = [Source].[ServerId]
                                                                             AND  [Target].[ChannelId] = [Source].[ChannelId]
                                                                             AND  [Target].[MessageId] = [Source].[MessageId]
                                                               WHEN NOT MATCHED THEN
                                                                          INSERT ( [DiscordServerId], [DiscordChannelId], [DiscordMessageId], [DiscordAccountId], [TimeStamp], [IsBatchCommitted])
                                                                          VALUES ( [Source].[DiscordServerId], [Source].[DiscordChannelId], [Source].[DiscordMessageId], [Source].[DiscordAccountId], [Source].[TimeStamp], 0); ",
                                                                    connection);

                        await using (sqlCommand.ConfigureAwait(false))
                        {
                            sqlCommand.ExecuteNonQuery();
                        }
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
}
