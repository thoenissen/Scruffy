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
                await using (var connection = new SqlConnection(GetDbContext().ConnectionString))
                {
                    await connection.OpenAsync()
                                    .ConfigureAwait(false);

                    await using (var sqlCommand = new SqlCommand(@"CREATE TABLE #DiscordMessages (
                                                                       [ServerId] decimal(20,0) NOT NULL,
                                                                       [ChannelId] decimal(20,0) NOT NULL,
                                                                       [MessageId] decimal(20,0) NOT NULL,
                                                                       [UserId] decimal(20,0) NOT NULL,
                                                                       [TimeStamp] datetime2 NOT NULL,
                                                                   )",
                                                                 connection))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    var dataTable = new DataTable();
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.ServerId), typeof(ulong));
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.ChannelId), typeof(ulong));
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.MessageId), typeof(ulong));
                    dataTable.Columns.Add(nameof(DiscordMessageEntity.UserId), typeof(ulong));
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
                        await using (var sqlCommand = new SqlCommand(@"MERGE INTO [DiscordMessages] AS [TARGET]
                                                                               USING #DiscordMessages AS [Source]
                                                                                  ON [Target].[ServerId] = [Source].[ServerId]
                                                                                 AND  [Target].[ChannelId] = [Source].[ChannelId]
                                                                                 AND  [Target].[MessageId] = [Source].[MessageId]
                                                                   WHEN MATCHED THEN
                                                                              UPDATE 
                                                                                SET [Target].[IsBatchCommitted] = 1
                                                                   WHEN NOT MATCHED THEN
                                                                              INSERT ( [ServerId], [ChannelId], [MessageId], [UserId], [TimeStamp], [IsBatchCommitted])
                                                                              VALUES ( [Source].[ServerId], [Source].[ChannelId], [Source].[MessageId], [Source].[UserId], [Source].[TimeStamp], 1); ",
                                                                     connection))
                        {
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        await using (var sqlCommand = new SqlCommand(@"MERGE INTO [DiscordMessages] AS [TARGET]
                                                                               USING #DiscordMessages AS [Source]
                                                                                  ON [Target].[ServerId] = [Source].[ServerId]
                                                                                 AND  [Target].[ChannelId] = [Source].[ChannelId]
                                                                                 AND  [Target].[MessageId] = [Source].[MessageId]
                                                                   WHEN NOT MATCHED THEN
                                                                              INSERT ( [ServerId], [ChannelId], [MessageId], [UserId], [TimeStamp], [IsBatchCommitted])
                                                                              VALUES ( [Source].[ServerId], [Source].[ChannelId], [Source].[MessageId], [Source].[UserId], [Source].[TimeStamp], 0); ",
                                                                     connection))
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
