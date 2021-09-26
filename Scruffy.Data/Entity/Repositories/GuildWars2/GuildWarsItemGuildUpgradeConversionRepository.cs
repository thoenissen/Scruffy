using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Items;

namespace Scruffy.Data.Entity.Repositories.GuildWars2
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsItemGuildUpgradeConversionEntity"/>
    /// </summary>
    public class GuildWarsItemGuildUpgradeConversionRepository : RepositoryBase<GuildWarsItemGuildUpgradeConversionQueryable, GuildWarsItemGuildUpgradeConversionEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildWarsItemGuildUpgradeConversionRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Bulk insert items
        /// </summary>
        /// <param name="entries">Items</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> BulkInsert(List<KeyValuePair<int, long>> entries)
        {
            var success = false;

            LastError = null;

            try
            {
                await using (var connection = new SqlConnection(GetDbContext().ConnectionString))
                {
                    await connection.OpenAsync()
                                    .ConfigureAwait(false);

                    await using (var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsItemGuildUpgradeConversions (
                                                                       [ItemId] int NOT NULL,
                                                                       [UpgradeId] bigint NOT NULL
                                                                   )",
                                                                 connection))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    var dataTable = new DataTable();
                    dataTable.Columns.Add(nameof(GuildWarsItemGuildUpgradeConversionEntity.ItemId), typeof(int));
                    dataTable.Columns.Add(nameof(GuildWarsItemGuildUpgradeConversionEntity.UpgradeId), typeof(long));

                    foreach (var entry in entries)
                    {
                        dataTable.Rows.Add(entry.Key, entry.Value);
                    }

                    using (var bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = "#GuildWarsItemGuildUpgradeConversions";

                        await bulk.WriteToServerAsync(dataTable)
                                  .ConfigureAwait(false);
                    }

                    await using (var sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsItemGuildUpgradeConversions] AS [TARGET]
                                                                               USING #GuildWarsItemGuildUpgradeConversions AS [Source]
                                                                                  ON [Target].[ItemId] = [Source].[ItemId]
                                                                                 AND  [Target].[UpgradeId] = [Source].[UpgradeId]
                                                               WHEN NOT MATCHED THEN
                                                                              INSERT ( [ItemId], [UpgradeId])
                                                                              VALUES ( [Source].[ItemId], [Source].[UpgradeId]); ",
                                                                 connection))
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
}
