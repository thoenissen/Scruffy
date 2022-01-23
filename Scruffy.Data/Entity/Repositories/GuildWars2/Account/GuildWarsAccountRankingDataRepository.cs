using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.Account;

/// <summary>
/// Repository for accessing <see cref="GuildWarsAccountRankingDataEntity"/>
/// </summary>
public class GuildWarsAccountRankingDataRepository : RepositoryBase<GuildWarsAccountRankingDataQueryable, GuildWarsAccountRankingDataEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildWarsAccountRankingDataRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Insert current achievement points
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task InsertCurrentAchievementPoints()
    {
        return GetDbContext().Database
                             .ExecuteSqlRawAsync(@"WITH [CurrentPoints]
                                                   AS
                                                   (
                                                          SELECT [AccountName],
                                                                 CONVERT ( date, GETDATE() ) AS [Date],
                                                                 ( [SummedAchievementPoints]
                                                                 + [DailyAchievementPoints]
                                                                 + [MonthlyAchievementPoints] ) AS [AchievementPoints]
                                                          
                                                            FROM ( SELECT [AccountName],
                                                                          SUM ( ( CASE
                                                                                       WHEN [PointCap] = -1 THEN 0
                                                                                       WHEN [CurrentPoints] + [RepetitionPoints] > [PointCap] THEN [PointCap]
                                                                                       ELSE [CurrentPoints] + [RepetitionPoints]
                                                                                  END ) ) AS [SummedAchievementPoints]
                                                                   
                                                                     FROM ( SELECT [AccountAchievement].[AccountName],
                                                                                   COALESCE ( ( SELECT SUM ( [CurrentTiers].[Points] )
                                                                                                  FROM [GuildWarsAchievementTiers] AS [CurrentTiers]
                                                                                                 WHERE [CurrentTiers].[AchievementId] = [AccountAchievement].AchievementId
                                                                                                   AND ( [CurrentTiers].[Count] <= [AccountAchievement].[Current]
                                                                                                    OR ( [AccountAchievement].[IsDone] = 1
                                                                                                   AND NOT EXISTS ( SELECT 1
                                                                                                                      FROM [GuildWarsAchievementFlags] AS [CurrentFlags]
                                                                                                                     WHERE [CurrentFlags].[AchievementId] = [CurrentTiers].[AchievementId]
                                                                                                                       AND [CurrentFlags].[Flag] = 'Repeatable' ) ) ) ), 0) AS [CurrentPoints],
                                                                                   COALESCE ( ( SELECT SUM ( [RepetitionTiers].[Points] )
                                                                                                  FROM [GuildWarsAchievementTiers] AS [RepetitionTiers]
                                                                                                 WHERE EXISTS ( SELECT 1
                                                                                                                  FROM [GuildWarsAchievementFlags] AS [RepetitionFlags]
                                                                                                                 WHERE [RepetitionFlags].[AchievementId] = [RepetitionTiers].[AchievementId]
                                                                                                                   AND [RepetitionFlags].[Flag] = 'Repeatable' )
                                                                                                                   AND [RepetitionTiers].[AchievementId] = [AccountAchievement].[AchievementId])
                                                                                              * [AccountAchievement].[RepetitionCount], 0) AS [RepetitionPoints],
                                                                                  [Achievement].[PointCap]
                                                                                   
                                                                             FROM [GuildWarsAccountAchievements] AS [AccountAchievement]
                                                                       INNER JOIN [GuildWarsAchievements] AS [Achievement]
                                                                               ON [Achievement].[Id] = [AccountAchievement].[AchievementId] ) AS [RawData]
                                                                         
                                                                 GROUP BY [AccountName] ) AS [SummedData]
                                                       
                                                       LEFT JOIN [GuildWarsAccounts] AS [Account]
                                                              ON [Account].[Name] = [SummedData].[AccountName]
                                                   )
                                                   
                                                   MERGE INTO [GuildWarsAccountRankingData] AS [Target]
                                                        USING [CurrentPoints] AS [Source]
                                                           ON [Target].[AccountName] = [Source].[AccountName]
                                                          AND [Target].[Date] = [Source].[Date]
                                                   
                                                          WHEN MATCHED
                                                               THEN UPDATE
                                                                   SET [Target].[AchievementPoints] = [Source].[AchievementPoints]
                                                          WHEN NOT MATCHED
                                                               THEN 
                                                                   INSERT ( [AccountName], [Date], [AchievementPoints] )
                                                                   VALUES ( [Source].[AccountName], [Source].[Date], [Source].[AchievementPoints] );");
    }

    #endregion // Methods
}