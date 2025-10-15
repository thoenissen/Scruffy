using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;
using Scruffy.Data.Json.GuildWars2.Account;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.Account;

/// <summary>
/// Repository for accessing <see cref="GuildWarsAccountAchievementEntity"/>
/// </summary>
public class GuildWarsAccountAchievementRepository : RepositoryBase<GuildWarsAccountAchievementQueryable, GuildWarsAccountAchievementEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildWarsAccountAchievementRepository(ScruffyDbContext dbContext)
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
    public async Task<bool> BulkInsert(string accountName, List<AccountAchievement> entries)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsAccountAchievements (
                                                                       [AccountName] nvarchar(42) NOT NULL,
                                                                       [AchievementId] int NOT NULL,
                                                                       [Current] int NULL,
                                                                       [Maximum] int NULL,
                                                                       [IsDone] bit NOT NULL,
                                                                       [RepetitionCount] int NULL,
                                                                       [IsUnlocked] bit NOT NULL
                                                                   );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"CREATE TABLE #GuildWarsAccountAchievementBits (
                                                                    [AccountName] nvarchar(42) NOT NULL,
                                                                    [AchievementId] int NOT NULL,
                                                                    [Bit] int NOT NULL
                                                                );";
                    sqlCommand.ExecuteNonQuery();
                }

                var achievementsTable = new DataTable();
                achievementsTable.Columns.Add(nameof(GuildWarsAccountAchievementEntity.AccountName), typeof(string));
                achievementsTable.Columns.Add(nameof(GuildWarsAccountAchievementEntity.AchievementId), typeof(int));
                achievementsTable.Columns.Add(nameof(GuildWarsAccountAchievementEntity.Current), typeof(int));
                achievementsTable.Columns.Add(nameof(GuildWarsAccountAchievementEntity.Maximum), typeof(int));
                achievementsTable.Columns.Add(nameof(GuildWarsAccountAchievementEntity.IsDone), typeof(bool));
                achievementsTable.Columns.Add(nameof(GuildWarsAccountAchievementEntity.RepetitionCount), typeof(int));
                achievementsTable.Columns.Add(nameof(GuildWarsAccountAchievementEntity.IsUnlocked), typeof(bool));

                var bitsTable = new DataTable();
                bitsTable.Columns.Add(nameof(GuildWarsAccountAchievementBitEntity.AccountName), typeof(string));
                bitsTable.Columns.Add(nameof(GuildWarsAccountAchievementBitEntity.AchievementId), typeof(int));
                bitsTable.Columns.Add(nameof(GuildWarsAccountAchievementBitEntity.Bit), typeof(int));

                foreach (var entry in entries)
                {
                    // Achievement
                    achievementsTable.Rows.Add(accountName,
                                               entry.Id,
                                               entry.Current == null
                                                   ? DBNull.Value
                                                   : entry.Current.Value,
                                               entry.Maximum == null
                                                   ? DBNull.Value
                                                   : entry.Maximum.Value,
                                               entry.IsDone,
                                               entry.RepetitionCount == null
                                                   ? DBNull.Value
                                                   : entry.RepetitionCount.Value,
                                               entry.IsUnlocked ?? true);

                    // Bits
                    if (entry.Bits != null)
                    {
                        foreach (var bit in entry.Bits)
                        {
                            bitsTable.Rows.Add(accountName,
                                               entry.Id,
                                               bit);
                        }
                    }
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#GuildWarsAccountAchievements";
                    await bulk.WriteToServerAsync(achievementsTable)
                              .ConfigureAwait(false);

                    bulk.DestinationTableName = "#GuildWarsAccountAchievementBits";
                    await bulk.WriteToServerAsync(bitsTable)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsAccountAchievements] AS [TARGET]
                                                                    USING #GuildWarsAccountAchievements AS [Source]
                                                                       ON [Target].[AccountName] = [Source].[AccountName]
                                                                      AND [Target].[AchievementId] = [Source].[AchievementId]
                                                           WHEN     MATCHED THEN
                                                                          UPDATE 
                                                                            SET [Target].[Current] = [Source].[Current],
                                                                                [Target].[Maximum] = [Source].[Maximum],
                                                                                [Target].[IsDone] = [Source].[IsDone],
                                                                                [Target].[RepetitionCount] = [Source].[RepetitionCount],
                                                                                [Target].[IsUnlocked] = [Source].[IsUnlocked]
                                                           WHEN NOT MATCHED THEN
                                                                          INSERT ( [AccountName], [AchievementId], [Current], [Maximum], [IsDone], [RepetitionCount], [IsUnlocked] )
                                                                          VALUES ( [Source].[AccountName], [Source].[AchievementId], [Source].[Current], [Source].[Maximum], [Source].[IsDone], [Source].[RepetitionCount], [Source].[IsUnlocked] );",
                                            connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"MERGE INTO [GuildWarsAccountAchievementBits] AS [TARGET]
                                                         USING #GuildWarsAccountAchievementBits AS [Source]
                                                            ON [Target].[AccountName] = [Source].[AccountName]
                                                           AND [Target].[AchievementId] = [Source].[AchievementId]
                                                           AND [Target].[Bit] = [Source].[Bit]
                                                WHEN NOT MATCHED THEN
                                                               INSERT ( [AccountName], [AchievementId], [Bit] )
                                                               VALUES ( [Source].[AccountName], [Source].[AchievementId], [Source].[Bit] )
                                                WHEN NOT MATCHED BY SOURCE 
                                                 AND EXISTS ( SELECT 1 FROM #GuildWarsAccountAchievements AS [EX] WHERE [EX].[AccountName] = [Target].[AccountName] AND [EX].[AchievementId] = [Target].[AchievementId] )
                                                THEN DELETE;";

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