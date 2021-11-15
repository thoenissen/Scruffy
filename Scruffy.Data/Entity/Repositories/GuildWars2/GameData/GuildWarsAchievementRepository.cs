using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.GameData;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;
using Scruffy.Data.Json.GuildWars2.Achievements;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.GameData;

/// <summary>
/// Repository for accessing <see cref="GuildWarsAchievementEntity"/>
/// </summary>
public class GuildWarsAchievementRepository : RepositoryBase<GuildWarsAchievementQueryable, GuildWarsAchievementEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildWarsAchievementRepository(ScruffyDbContext dbContext)
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
    public async Task<bool> BulkInsert(List<Achievement> entries)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsAchievements (
                                                                       [Id] int NOT NULL,
                                                                       [Icon] nvarchar(max) NULL,
                                                                       [Name] nvarchar(max) NULL,
                                                                       [Description] nvarchar(max) NULL,
                                                                       [Requirement] nvarchar(max) NULL,
                                                                       [LockedText] nvarchar(max) NULL,
                                                                       [Type] nvarchar(max) NULL,
                                                                       [PointCap] int NULL
                                                                   );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"CREATE TABLE #GuildWarsAchievementBits (
                                                                       [AchievementId] int NOT NULL,
                                                                       [Bit] int NOT NULL,
                                                                       [Id] int NULL,
                                                                       [Type] nvarchar(max) NULL,
                                                                       [Text] nvarchar(max) NULL
                                                                   );";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"CREATE TABLE #GuildWarsAchievementFlags (
                                                       [AchievementId] int NOT NULL,
                                                       [Flag] nvarchar(50) NOT NULL
                                                   );";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"CREATE TABLE #GuildWarsAchievementPrerequisites (
                                                       [AchievementId] int NOT NULL,
                                                       [Id] int NOT NULL
                                                   );";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"CREATE TABLE #GuildWarsAchievementRewards (
                                                       [AchievementId] int NOT NULL,
                                                       [Counter] int NOT NULL,
                                                       [Id] int NOT NULL,
                                                       [Type] nvarchar(max) NULL,
                                                       [Count] int NOT NULL,
                                                       [Region] nvarchar(max) NULL
                                                   );";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"CREATE TABLE #GuildWarsAchievementTiers (
                                                       [AchievementId] int NOT NULL,
                                                       [Counter] int NOT NULL,
                                                       [Count] int NOT NULL,
                                                       [Points] int NOT NULL
                                                   );";
                    sqlCommand.ExecuteNonQuery();
                }

                var achievementsTable = new DataTable();
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.Id), typeof(int));
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.Icon), typeof(string));
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.Name), typeof(string));
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.Description), typeof(string));
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.Requirement), typeof(string));
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.LockedText), typeof(string));
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.Type), typeof(string));
                achievementsTable.Columns.Add(nameof(GuildWarsAchievementEntity.PointCap), typeof(int));

                var bitsTable = new DataTable();
                bitsTable.Columns.Add(nameof(GuildWarsAchievementBitEntity.AchievementId), typeof(int));
                bitsTable.Columns.Add(nameof(GuildWarsAchievementBitEntity.Bit), typeof(int));
                bitsTable.Columns.Add(nameof(GuildWarsAchievementBitEntity.Id), typeof(int));
                bitsTable.Columns.Add(nameof(GuildWarsAchievementBitEntity.Type), typeof(string));
                bitsTable.Columns.Add(nameof(GuildWarsAchievementBitEntity.Text), typeof(string));

                var flagsTable = new DataTable();
                flagsTable.Columns.Add(nameof(GuildWarsAchievementFlagEntity.AchievementId), typeof(int));
                flagsTable.Columns.Add(nameof(GuildWarsAchievementFlagEntity.Flag), typeof(string));

                var prerequisitesTable = new DataTable();
                prerequisitesTable.Columns.Add(nameof(GuildWarsAchievementPrerequisiteEntity.AchievementId), typeof(int));
                prerequisitesTable.Columns.Add(nameof(GuildWarsAchievementPrerequisiteEntity.Id), typeof(int));

                var rewardsTable = new DataTable();
                rewardsTable.Columns.Add(nameof(GuildWarsAchievementRewardEntity.AchievementId), typeof(int));
                rewardsTable.Columns.Add(nameof(GuildWarsAchievementRewardEntity.Counter), typeof(int));
                rewardsTable.Columns.Add(nameof(GuildWarsAchievementRewardEntity.Id), typeof(int));
                rewardsTable.Columns.Add(nameof(GuildWarsAchievementRewardEntity.Type), typeof(string));
                rewardsTable.Columns.Add(nameof(GuildWarsAchievementRewardEntity.Count), typeof(int));
                rewardsTable.Columns.Add(nameof(GuildWarsAchievementRewardEntity.Region), typeof(string));

                var tiersTable = new DataTable();
                tiersTable.Columns.Add(nameof(GuildWarsAchievementTierEntity.AchievementId), typeof(int));
                tiersTable.Columns.Add(nameof(GuildWarsAchievementTierEntity.Counter), typeof(int));
                tiersTable.Columns.Add(nameof(GuildWarsAchievementTierEntity.Count), typeof(int));
                tiersTable.Columns.Add(nameof(GuildWarsAchievementTierEntity.Points), typeof(int));

                foreach (var entry in entries)
                {
                    // Achievement
                    achievementsTable.Rows.Add(entry.Id,
                                               entry.Icon,
                                               entry.Name,
                                               entry.Description,
                                               entry.Requirement,
                                               entry.LockedText,
                                               entry.Type,
                                               entry.PointCap == null
                                                   ? DBNull.Value
                                                   : entry.PointCap.Value);

                    // Bits
                    if (entry.Bits != null)
                    {
                        var bitCounter = 0;

                        foreach (var bit in entry.Bits)
                        {
                            bitsTable.Rows.Add(entry.Id,
                                               bitCounter,
                                               bit.Id == null
                                                   ? DBNull.Value
                                                   : bit.Id.Value,
                                               bit.Type,
                                               bit.Text);

                            bitCounter++;
                        }
                    }

                    // Flags
                    if (entry.Flags != null)
                    {
                        foreach (var flag in entry.Flags)
                        {
                            flagsTable.Rows.Add(entry.Id, flag);
                        }
                    }

                    // Prerequisites
                    if (entry.Prerequisites != null)
                    {
                        foreach (var prerequisite in entry.Prerequisites)
                        {
                            prerequisitesTable.Rows.Add(entry.Id, prerequisite);
                        }
                    }

                    // Rewards
                    if (entry.Rewards != null)
                    {
                        var rewardCounter = 0;

                        foreach (var reward in entry.Rewards)
                        {
                            rewardsTable.Rows.Add(entry.Id, rewardCounter, reward.Id, reward.Type, reward.Count, reward.Region);

                            rewardCounter++;
                        }
                    }

                    // Tiers
                    if (entry.Tiers != null)
                    {
                        var tierCounter = 0;

                        foreach (var tier in entry.Tiers)
                        {
                            tiersTable.Rows.Add(entry.Id, tierCounter, tier.Count, tier.Points);

                            tierCounter++;
                        }
                    }
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#GuildWarsAchievements";
                    await bulk.WriteToServerAsync(achievementsTable)
                              .ConfigureAwait(false);

                    bulk.DestinationTableName = "#GuildWarsAchievementBits";
                    await bulk.WriteToServerAsync(bitsTable)
                              .ConfigureAwait(false);

                    bulk.DestinationTableName = "#GuildWarsAchievementFlags";
                    await bulk.WriteToServerAsync(flagsTable)
                              .ConfigureAwait(false);

                    bulk.DestinationTableName = "#GuildWarsAchievementPrerequisites";
                    await bulk.WriteToServerAsync(prerequisitesTable)
                              .ConfigureAwait(false);

                    bulk.DestinationTableName = "#GuildWarsAchievementRewards";
                    await bulk.WriteToServerAsync(rewardsTable)
                              .ConfigureAwait(false);

                    bulk.DestinationTableName = "#GuildWarsAchievementTiers";
                    await bulk.WriteToServerAsync(tiersTable)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsAchievements] AS [TARGET]
                                                                    USING #GuildWarsAchievements AS [Source]
                                                                       ON [Target].[Id] = [Source].[Id]
                                                           WHEN     MATCHED THEN
                                                                          UPDATE 
                                                                            SET [Target].[Icon] = [Source].[Icon],
                                                                                [Target].[Name] = [Source].[Name],
                                                                                [Target].[Description] = [Source].[Description],
                                                                                [Target].[Requirement] = [Source].[Requirement],
                                                                                [Target].[LockedText] = [Source].[LockedText],
                                                                                [Target].[Type] = [Source].[Type],
                                                                                [Target].[PointCap] = [Source].[PointCap]
                                                           WHEN NOT MATCHED THEN
                                                                          INSERT ( [Id], [Icon], [Name], [Description], [Requirement], [LockedText], [Type], [PointCap] )
                                                                          VALUES ( [Source].[Id], [Source].[Icon], [Source].[Name], [Source].[Description], [Source].[Requirement], [Source].[LockedText], [Source].[Type], [Source].[PointCap] );",
                                            connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"MERGE INTO [GuildWarsAchievementBits] AS [TARGET]
                                                         USING #GuildWarsAchievementBits AS [Source]
                                                            ON [Target].[AchievementId] = [Source].[AchievementId]
                                                           AND [Target].[Bit] = [Source].[Bit]
                                                WHEN     MATCHED THEN
                                                               UPDATE 
                                                                 SET [Target].[Id] = [Source].[Id],
                                                                     [Target].[Type] = [Source].[Type],
                                                                     [Target].[Text] = [Source].[Text]
                                                WHEN NOT MATCHED THEN
                                                               INSERT ( [AchievementId], [Bit], [Id], [Type], [Text] )
                                                               VALUES ( [Source].[AchievementId], [Source].[Bit], [Source].[Id], [Source].[Type], [Source].[Text] )
                                                WHEN NOT MATCHED BY SOURCE 
                                                 AND EXISTS ( SELECT 1 FROM #GuildWarsAchievements AS [EX] WHERE [EX].[Id] = [Target].[AchievementId] )
                                                THEN DELETE;";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"MERGE INTO [GuildWarsAchievementFlags] AS [TARGET]
                                                        USING #GuildWarsAchievementFlags AS [Source]
                                                           ON [Target].[AchievementId] = [Source].[AchievementId]
                                                          AND [Target].[Flag] = [Source].[Flag]
                                               WHEN NOT MATCHED THEN
                                                              INSERT ( [AchievementId], [Flag] )
                                                              VALUES ( [Source].[AchievementId], [Source].[Flag] )
                                               WHEN NOT MATCHED BY SOURCE 
                                                AND EXISTS ( SELECT 1 FROM #GuildWarsAchievements AS [EX] WHERE [EX].[Id] = [Target].[AchievementId] )
                                               THEN DELETE;";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"MERGE INTO [GuildWarsAchievementPrerequisites] AS [TARGET]
                                                        USING #GuildWarsAchievementPrerequisites AS [Source]
                                                           ON [Target].[AchievementId] = [Source].[AchievementId]
                                                          AND [Target].[Id] = [Source].[Id]
                                               WHEN NOT MATCHED THEN
                                                              INSERT ( [AchievementId], [Id] )
                                                              VALUES ( [Source].[AchievementId], [Source].[Id] )
                                               WHEN NOT MATCHED BY SOURCE 
                                                AND EXISTS ( SELECT 1 FROM #GuildWarsAchievements AS [EX] WHERE [EX].[Id] = [Target].[AchievementId] )
                                               THEN DELETE;";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"MERGE INTO [GuildWarsAchievementRewards] AS [TARGET]
                                                        USING #GuildWarsAchievementRewards AS [Source]
                                                           ON [Target].[AchievementId] = [Source].[AchievementId]
                                                          AND [Target].[Counter] = [Source].[Counter]
                                               WHEN NOT MATCHED THEN
                                                              INSERT ( [AchievementId], [Counter], [Id], [Type], [Count], [Region] )
                                                              VALUES ( [Source].[AchievementId], [Source].[Counter], [Source].[Id], [Source].[Type], [Source].[Count], [Source].[Region] )
                                               WHEN NOT MATCHED BY SOURCE 
                                                AND EXISTS ( SELECT 1 FROM #GuildWarsAchievements AS [EX] WHERE [EX].[Id] = [Target].[AchievementId] )
                                               THEN DELETE;";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"MERGE INTO [GuildWarsAchievementTiers] AS [TARGET]
                                                        USING #GuildWarsAchievementTiers AS [Source]
                                                           ON [Target].[AchievementId] = [Source].[AchievementId]
                                                          AND [Target].[Counter] = [Source].[Counter]
                                               WHEN NOT MATCHED THEN
                                                              INSERT ( [AchievementId], [Counter], [Count], [Points] )
                                                              VALUES ( [Source].[AchievementId], [Source].[Counter], [Source].[Count], [Source].[Points] )
                                               WHEN NOT MATCHED BY SOURCE 
                                                AND EXISTS ( SELECT 1 FROM #GuildWarsAchievements AS [EX] WHERE [EX].[Id] = [Target].[AchievementId] )
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