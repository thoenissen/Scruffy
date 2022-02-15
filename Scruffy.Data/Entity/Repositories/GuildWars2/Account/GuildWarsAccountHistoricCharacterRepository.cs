using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;
using Scruffy.Data.Json.GuildWars2.Characters;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.Account
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsAccountHistoricCharacterEntity"/>
    /// </summary>
    public class GuildWarsAccountHistoricCharacterRepository : RepositoryBase<GuildWarsAccountHistoricCharacterQueryable, GuildWarsAccountHistoricCharacterEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildWarsAccountHistoricCharacterRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Bulk insert
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <param name="entries">Entries</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> BulkInsert(string accountName, List<Character> entries)
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

                    var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsAccountHistoricCharacters (
                                                                       [Date] datetime2(7) NOT NULL,
                                                                       [GuildId] nvarchar(MAX) NOT NULL,
                                                                       [AccountName] nvarchar(42) NOT NULL,
                                                                       [CharacterName] nvarchar(MAX) NOT NULL
                                                                   );",
                                                    connection);

                    await using (sqlCommand.ConfigureAwait(false))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    var table = new DataTable();
                    table.Columns.Add(nameof(GuildWarsAccountHistoricCharacterEntity.Date), typeof(DateTime));
                    table.Columns.Add(nameof(GuildWarsAccountHistoricCharacterEntity.GuildId), typeof(string));
                    table.Columns.Add(nameof(GuildWarsAccountHistoricCharacterEntity.AccountName), typeof(string));
                    table.Columns.Add(nameof(GuildWarsAccountHistoricCharacterEntity.CharacterName), typeof(string));

                    var today = DateTime.Today;

                    foreach (var entry in entries)
                    {
                        // Achievement
                        table.Rows.Add(today, entry.Guild, accountName, entry.Name);
                    }

                    using (var bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = "#GuildWarsAccountHistoricCharacters";
                        await bulk.WriteToServerAsync(table)
                                  .ConfigureAwait(false);
                    }

                    sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsAccountHistoricCharacters] AS [TARGET]
                                                                    USING #GuildWarsAccountHistoricCharacters AS [Source]
                                                                       ON [Target].[Date] = [Source].[Date]
                                                                      AND [Target].[GuildId] = [Source].[GuildId]
                                                                      AND [Target].[AccountName] = [Source].[AccountName]
                                                                      AND [Target].[CharacterName] = [Source].[CharacterName]
                                                           WHEN NOT MATCHED THEN
                                                                INSERT ( [Date], [GuildId], [AccountName], [CharacterName] )
                                                                VALUES ( [Source].[Date], [Source].[GuildId], [Source].[AccountName], [Source].[CharacterName] )
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
}
