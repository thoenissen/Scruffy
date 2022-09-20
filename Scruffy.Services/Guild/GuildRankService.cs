using Discord;
using Discord.WebSocket;

using Microsoft.Data.SqlClient;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.GuildWars2.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild;

/// <summary>
/// Guild rank management
/// </summary>
public class GuildRankService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Discord-Client
    /// </summary>
    private readonly DiscordSocketClient _discordClient;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="discordClient">Discord-Client</param>
    /// <param name="repositoryFactory">Repository factory</param>
    public GuildRankService(LocalizationService localizationService, DiscordSocketClient discordClient, RepositoryFactory repositoryFactory)
        : base(localizationService)
    {
        _discordClient = discordClient;
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Import all in game ranks
    /// </summary>
    /// <param name="guildId">Id of the guild</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ImportGuildRanks(long? guildId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            foreach (var guild in dbFactory.GetRepository<GuildRepository>()
                                           .GetQuery()
                                           .Where(obj => guildId == null
                                                      || obj.Id == guildId)
                                           .Select(obj => new
                                                          {
                                                              obj.Id,
                                                              obj.GuildId,
                                                              obj.ApiKey
                                                          })
                                           .ToList())
            {
                var connector = new GuildWars2ApiConnector(guild.ApiKey);
                await using (connector.ConfigureAwait(false))
                {
                    List<GuildMember> members;

                    try
                    {
                        members = await connector.GetGuildMembers(guild.GuildId)
                                                 .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Determination of guild members failed", null, ex);

                        var yesterday = DateTime.Today.AddDays(-1);

                        members = dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.Date == yesterday
                                                      && obj.GuildId == guild.Id)
                                           .Select(obj => new GuildMember
                                                          {
                                                              Name = obj.Name,
                                                              Rank = obj.Rank,
                                                              Joined = obj.JoinedAt
                                                          })
                                           .ToList();
                    }

                    await dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                   .BulkInsert(guild.Id, members.Select(obj => (obj.Name, obj.Rank, obj.Joined)))
                                   .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Refreshing the member rank
    /// </summary>
    /// <param name="guildId">Id of the guild</param>
    /// <param name="accountName">Account name</param>
    /// <param name="rankName">rank</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> RefreshDiscordRank(long guildId, string accountName, string rankName)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var isChanged = false;

            var today = DateTime.Today;

            dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                     .AddOrRefresh(obj => obj.GuildId == guildId
                                       && obj.Name == accountName
                                       && obj.Date == today,
                                   obj =>
                                   {
                                       obj.GuildId = guildId;
                                       obj.Name = accountName;
                                       obj.Date = today;

                                       isChanged = obj.Rank != rankName;

                                       obj.Rank = rankName;
                                   });

            if (isChanged)
            {
                var guildMemberQuery = dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                .GetQuery()
                                                .Select(obj => obj);

                var guildWarsAccountQuery = dbFactory.GetRepository<GuildWarsAccountRepository>()
                                                     .GetQuery()
                                                     .Select(obj => obj);

                var guildRankQuery = dbFactory.GetRepository<GuildRankRepository>()
                                              .GetQuery()
                                              .Select(obj => obj);

                var discordAccountQuery = dbFactory.GetRepository<DiscordAccountRepository>()
                                                   .GetQuery()
                                                   .Select(obj => obj);

                var guild = dbFactory.GetRepository<GuildRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.Id == guildId)
                                     .Select(obj => new
                                                    {
                                                        obj.Id,
                                                        obj.DiscordServerId,
                                                        Roles = guildRankQuery.Where(obj2 => obj2.GuildId == obj.Id)
                                                                              .Select(obj2 => obj2.DiscordRoleId)
                                                                              .ToList()
                                                    })
                                     .FirstOrDefault();

                if (guild != null)
                {
                    var discordServer = _discordClient.GetGuild(guild.DiscordServerId);

                    var users = guildWarsAccountQuery.Where(obj => obj.Name == accountName)
                                                     .Join(guildWarsAccountQuery,
                                                           obj => obj.UserId,
                                                           obj => obj.UserId,
                                                           (obj1, obj2) => new
                                                                           {
                                                                               obj2.UserId,
                                                                               obj2.Name,
                                                                           })
                                                     .Join(guildMemberQuery.Where(obj => guildMemberQuery.Any(obj2 => obj2.GuildId == obj.GuildId
                                                                                                                   && obj2.Date > obj.Date) == false),
                                                           obj => obj.Name,
                                                           obj => obj.Name,
                                                           (obj1, obj2) => new
                                                                           {
                                                                               obj1.UserId,
                                                                               obj2.Rank
                                                                           })
                                                     .Join(guildRankQuery.Where(obj => obj.GuildId == guild.Id),
                                                           obj => obj.Rank,
                                                           obj => obj.InGameName,
                                                           (obj1, obj2) => new
                                                                           {
                                                                               obj1.UserId,
                                                                               obj2.Order
                                                                           })
                                                     .GroupBy(obj => obj.UserId)
                                                     .Select(obj => new
                                                                    {
                                                                        UserId = obj.Key,
                                                                        Order = obj.Min(obj2 => obj2.Order)
                                                                    })
                                                     .Join(discordAccountQuery,
                                                           obj => obj.UserId,
                                                           obj => obj.UserId,
                                                           (obj1, obj2) => new
                                                                           {
                                                                               DiscordUserId = obj2.Id,
                                                                               DiscordRoleId = guildRankQuery.Where(obj => obj.GuildId == guild.Id
                                                                                                                        && obj.Order == obj1.Order)
                                                                                                             .Select(obj => obj.DiscordRoleId)
                                                                                                             .FirstOrDefault()
                                                                           })
                                                     .Distinct()
                                                     .ToList();

                    foreach (var user in users)
                    {
                        var member = discordServer.GetUser(user.DiscordUserId);

                        foreach (var role in member.Roles.ToList())
                        {
                            if (user.DiscordRoleId != role.Id
                             && guild.Roles.Contains(role.Id))
                            {
                                await member.RemoveRoleAsync(role)
                                            .ConfigureAwait(false);
                            }
                        }

                        if (member.Roles.Any(obj => obj.Id == user.DiscordRoleId) == false)
                        {
                            await member.AddRoleAsync(discordServer.GetRole(user.DiscordRoleId))
                                        .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Refresh discord roles
    /// </summary>
    /// <param name="guildId">Id of the guild</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RefreshDiscordRoles(long? guildId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var guildMemberQuery = dbFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                            .GetQuery()
                                            .Select(obj => obj);

            var guildWarsAccountQuery = dbFactory.GetRepository<GuildWarsAccountRepository>()
                                                 .GetQuery()
                                                 .Select(obj => obj);

            var guildRankQuery = dbFactory.GetRepository<GuildRankRepository>()
                                          .GetQuery()
                                          .Select(obj => obj);

            var discordAccountQuery = dbFactory.GetRepository<DiscordAccountRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

            foreach (var guild in dbFactory.GetRepository<GuildRepository>()
                                           .GetQuery()
                                           .Where(obj => guildId == null
                                                      || obj.Id == guildId)
                                           .Select(obj => new
                                                          {
                                                              obj.Id,
                                                              obj.DiscordServerId,
                                                              Roles = guildRankQuery.Where(obj2 => obj2.GuildId == obj.Id)
                                                                                    .Select(obj2 => obj2.DiscordRoleId)
                                                                                    .ToList()
                                                          })
                                           .ToList())
            {
                var users = guildMemberQuery.Where(obj => obj.GuildId == guild.Id
                                                       && guildMemberQuery.Any(obj2 => obj2.GuildId == obj.GuildId
                                                                                    && obj2.Date > obj.Date) == false)
                                            .Join(guildWarsAccountQuery,
                                                  obj => obj.Name,
                                                  obj => obj.Name,
                                                  (obj1, obj2) => new
                                                                  {
                                                                      obj1.Rank,
                                                                      obj2.UserId,
                                                                  })
                                            .Join(guildRankQuery.Where(obj => obj.GuildId == guild.Id),
                                                  obj => obj.Rank,
                                                  obj => obj.InGameName,
                                                  (obj1, obj2) => new
                                                                  {
                                                                      obj1.UserId,
                                                                      obj2.Order
                                                                  })
                                            .GroupBy(obj => obj.UserId)
                                            .Select(obj => new
                                                           {
                                                               UserId = obj.Key,
                                                               Order = obj.Min(obj2 => obj2.Order)
                                                           })
                                            .Join(discordAccountQuery,
                                                  obj => obj.UserId,
                                                  obj => obj.UserId,
                                                  (obj1, obj2) => new
                                                                  {
                                                                      DiscordUserId = obj2.Id,
                                                                      DiscordRoleId = guildRankQuery.Where(obj => obj.GuildId == guild.Id
                                                                                                               && obj.Order == obj1.Order)
                                                                                                    .Select(obj => obj.DiscordRoleId)
                                                                                                    .FirstOrDefault()
                                                                  })
                                            .Distinct()
                                            .ToDictionary(obj => obj.DiscordUserId,
                                                          obj => obj.DiscordRoleId);

                var discordServer = _discordClient.GetGuild(guild.DiscordServerId);

                await foreach (var members in discordServer.GetUsersAsync()
                                                           .ConfigureAwait(false))
                {
                    foreach (var member in members.Where(obj => obj.IsBot == false))
                    {
                        var assignedRoleId = default(ulong?);

                        if (users.TryGetValue(member.Id, out var discordRoleId))
                        {
                            assignedRoleId = discordRoleId;
                        }

                        foreach (var roleId in member.RoleIds)
                        {
                            if (assignedRoleId != roleId
                             && guild.Roles.Contains(roleId))
                            {
                                await member.RemoveRoleAsync(roleId)
                                            .ConfigureAwait(false);
                            }
                        }

                        if (assignedRoleId != null
                         && member.RoleIds.Any(obj => obj == assignedRoleId) == false)
                        {
                            await member.AddRoleAsync(discordServer.GetRole(assignedRoleId.Value))
                                        .ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Refresh current guild rank points
    /// </summary>
    /// <param name="guildId">Id of the guild</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RefreshCurrentPoints(long? guildId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var from = DateTime.Today.AddDays(-64);
            var to = DateTime.Today.AddDays(-1);

            foreach (var guild in dbFactory.GetRepository<GuildRepository>()
                                           .GetQuery()
                                           .Where(obj => guildId == null
                                                      || obj.Id == guildId)
                                           .Select(obj => new
                                                          {
                                                              obj.Id
                                                          })
                                           .ToList())
            {
                // Daily login
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentPoints]
                                                         AS
                                                         (
                                                             SELECT [DateRange].[Value] AS [Date],
                                                                    [GuildWarsAccount].[UserId], 
                                                                    CASE
                                                                        WHEN MAX([DailyLogin].[Date]) IS NULL THEN 0
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 28 THEN -0.57
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 21 THEN -0.43
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 14 THEN -0.29
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 7 THEN -0.11
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 3 THEN 0.11
                                                                        ELSE 0.18
                                                                    END AS [Points]
                                                         
                                                               FROM ( SELECT [Value] FROM [GetDateRange](@from, @to)) AS [DateRange]
                                                         
                                                          LEFT JOIN [GuildWarsGuildHistoricMembers] AS [Member]
                                                                 ON [Member].[Date] = [DateRange].[Value]
                                                         INNER JOIN [GuildWarsAccounts] AS [GuildWarsAccount]
                                                                 ON [Member].[Name] = [GuildWarsAccount].[Name]
                                                          LEFT JOIN [GuildWarsAccountDailyLoginChecks] AS [DailyLogin]
                                                                 ON [Member].[Name] = [DailyLogin].[Name]
                                                                AND [DateRange].[Value] >= [DailyLogin].[Date]
                                                         
                                                              WHERE [Member].[GuildId] = @guildId
                                                         
                                                           GROUP BY [DateRange].[Value], 
                                                                    [GuildWarsAccount].[UserId]
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 0
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 0, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                                   .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Daily login", null, dbFactory.LastError);
                }

                // Representation points
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentRepresentationPoints]
                                                         AS
                                                         (
                                                             SELECT [SummedDates].[Date],
                                                                    [SummedDates].[UserId], 
                                                                    CASE 
                                                                        WHEN EXISTS ( SELECT 1 
                                                                                        FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                       WHERE [Exists].[Date] = [SummedDates].[Date]
                                                                                         AND [Exists].[UserId] = [SummedDates].[UserId]
                                                                                         AND [Exists].[GuildId] = @guildId
                                                                                         AND [Exists].[Type] = 0
                                                                                         AND [Exists].[Points] > 0)
                                                                            THEN COALESCE ( (0.14 * [RepresentationPercentage] ), 0)
                                                                        ELSE 0
                                                                    END AS [Points]
                                                               FROM ( SELECT [RawDates].[UserId], 
                                                                             [RawDates].[Date],
                                                                             ( CAST ( NULLIF ( SUM( [RepresentationCount] ), 0) AS FLOAT ) 
                                                                                      / CAST ( SUM( [CharactersCount] ) AS FLOAT ) ) AS [RepresentationPercentage]
                                                                        FROM ( SELECT [CurrentPoints].[UserId], 
                                                                                      [CurrentPoints].[Date],
                                                                                      ( SELECT COUNT (*)
                                                                                         FROM [GuildWarsAccountHistoricCharacters] AS [CharactersCount] 
                                                                                    LEFT JOIN [Guilds] AS [CharacterGuild]
                                                                                           ON [CharacterGuild].[GuildId] = [CharactersCount].[GuildId]
                                                                                        WHERE [CharactersCount].[Date] = [CurrentPoints].[Date]
                                                                                          AND [CharactersCount].[AccountName] = [Account].[Name] 
                                                                                          AND [CharacterGuild].[Id] = @guildId )  AS [RepresentationCount],
                                                                                      ( SELECT COUNT (*)
                                                                                          FROM [GuildWarsAccountHistoricCharacters] AS [CharactersCount] 
                                                                                         WHERE [CharactersCount].[Date] = [CurrentPoints].[Date]
                                                                                           AND [CharactersCount].[AccountName] = [Account].[Name] ) AS [CharactersCount]
                                                                                 FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                           INNER JOIN [GuildWarsAccounts] AS [Account]
                                                                                   ON [Account].[UserId] = [CurrentPoints].[UserId]
                                                                                WHERE [CurrentPoints].[Type] = 0       
                                                                                  AND [CurrentPoints].[Date] >= @from
                                                                                  AND [CurrentPoints].[Date] <= @to
                                                                                  AND [CurrentPoints].[GuildId] = @guildId
                                                                             ) AS [RawDates]
                                                                   GROUP BY [RawDates].[UserId],
                                                                            [RawDates].[Date] ) as [SummedDates] 
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentRepresentationPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 1
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 1, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Representation points", null, dbFactory.LastError);
                }

                // Achievement points
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentAchievementPoints]
                                                         AS
                                                         (
                                                             SELECT [Dates].[Date],
                                                                    [Dates].[UserId], 
                                                                    CASE 
                                                                        WHEN EXISTS ( SELECT 1 
                                                                                        FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                       WHERE [Exists].[Date] = [Dates].[Date]
                                                                                         AND [Exists].[UserId] = [Dates].[UserId]
                                                                                         AND [Exists].[GuildId] = @guildId
                                                                                         AND [Exists].[Type] = 0
                                                                                         AND [Exists].[Points] > 0)
                                                                            THEN COALESCE ( 0.14 
                                                                                            / NULLIF ( ( SELECT COUNT (*) 
                                                                                                           FROM [GuildWarsAccountRankingData] AS [All]
                                                                                                          WHERE [All].[Date] = [Dates].[Date] 
                                                                                                            AND EXISTS ( SELECT 1 
                                                                                                                          FROM [GuildWarsGuildHistoricMembers] AS [AllMembers]
                                                                                                                         WHERE [AllMembers].[GuildId] = @guildId
                                                                                                                           AND [AllMembers].[Date] = [All].[Date] 
                                                                                                                           AND [AllMembers].[Name] = [All].[AccountName] ) ) - 1, 0)
                                                                                            * ( SELECT COUNT (*) 
                                                                                                  FROM [GuildWarsAccountRankingData] AS [Less]
                                                                                                 WHERE [Less].[Date] = [Dates].[Date]
                                                                                                   AND [Less].[AchievementPoints] < [Dates].[AchievementPoints] 
                                                                                                   AND EXISTS ( SELECT 1 
                                                                                                                 FROM [GuildWarsGuildHistoricMembers] AS [LessMembers]
                                                                                                                WHERE [LessMembers].[GuildId] = @guildId
                                                                                                                  AND [LessMembers].[Date] = [Less].[Date] 
                                                                                                                  AND [LessMembers].[Name] = [Less].[AccountName] ) ),
                                                                                          0)
                                                                        ELSE 0
                                                                    END AS [Points]
                                                               FROM ( SELECT [CurrentPoints].[UserId], 
                                                                             [CurrentPoints].[Date],
                                                                             MAX ( COALESCE ( [RankingData].[AchievementPoints], 0 ) ) AS [AchievementPoints]
                                                                        FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                  INNER JOIN [GuildWarsAccounts] AS [Account]
                                                                          ON [Account].[UserId] = [CurrentPoints].[UserId]
                                                                   LEFT JOIN [GuildWarsAccountRankingData] AS [RankingData]
                                                                          ON [RankingData].[AccountName] = [Account].[Name]
                                                                         AND [RankingData].[Date] = [CurrentPoints].[Date]
                                                                       WHERE [CurrentPoints].[Type] = 0       
                                                                         AND [CurrentPoints].[Date] >= @from
                                                                         AND [CurrentPoints].[Date] <= @to
                                                                         AND [CurrentPoints].[GuildId] = @guildId
                                                                    GROUP BY [CurrentPoints].[UserId],
                                                                             [CurrentPoints].[Date] ) AS [Dates]
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentAchievementPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 2
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 2, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Achievement points", null, dbFactory.LastError);
                }

                // Membership points
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentMembershipPoints]
                                                         AS
                                                         (
                                                             SELECT [Dates].[Date],
                                                                    [Dates].[UserId], 
                                                                    CASE 
                                                                        WHEN EXISTS ( SELECT 1 
                                                                                        FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                       WHERE [Exists].[Date] = [Dates].[Date]
                                                                                         AND [Exists].[UserId] = [Dates].[UserId]
                                                                                         AND [Exists].[GuildId] = @guildId
                                                                                         AND [Exists].[Type] = 0
                                                                                         AND [Exists].[Points] > 0)
                                                                            THEN COALESCE ( 0.14 
                                                                                            / NULLIF ( ( SELECT COUNT (*) 
                                                                                                           FROM [GuildWarsGuildHistoricMembers] AS [All]
                                                                                                          WHERE [All].[Date] = [Dates].[Date]
                                                                                                            AND [All].[GuildId] = @guildId ) - 1, 0)
                                                                                            * ( SELECT COUNT (*) 
                                                                                                           FROM [GuildWarsGuildHistoricMembers] AS [Greater]
                                                                                                          WHERE [Greater].[Date] = [Dates].[Date]
                                                                                                            AND [Greater].[GuildId] = @guildId
                                                                                                            AND [Greater].[JoinedAt] > [Dates].[JoinedAt] ),
                                                                                          0)
                                                                        ELSE 0
                                                                    END AS [Points]
                                                               FROM ( SELECT [CurrentPoints].[UserId], 
                                                                             [CurrentPoints].[Date],
                                                                             MIN ( [Member].[JoinedAt] ) AS [JoinedAt]
                                                                        FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                  INNER JOIN [GuildWarsAccounts] AS [Account]
                                                                          ON [Account].[UserId] = [CurrentPoints].[UserId]
                                                                   LEFT JOIN [GuildWarsGuildHistoricMembers] AS [Member]
                                                                          ON [Member].[Name] = [Account].[Name]
                                                                         AND [Member].[Date] = [CurrentPoints].[Date]
                                                                         AND [Member].[GuildId] = @guildId
                                                                       WHERE [CurrentPoints].[Type] = 0       
                                                                         AND [CurrentPoints].[Date] >= @from
                                                                         AND [CurrentPoints].[Date] <= @to
                                                                         AND [CurrentPoints].[GuildId] = @guildId
                                                                    GROUP BY [CurrentPoints].[UserId],
                                                                             [CurrentPoints].[Date] ) AS [Dates]
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentMembershipPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 3
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 3, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Membership points", null, dbFactory.LastError);
                }

                // Donations
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentDonationPoints]
                                                         AS
                                                         (
                                                             SELECT [Dates].[UserId], 
                                                                    [Dates].[Date],
                                                                    ( CASE 
                                                                         WHEN EXISTS ( SELECT 1 
                                                                                         FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                        WHERE [Exists].[Date] = [Dates].[Date]
                                                                                          AND [Exists].[UserId] = [Dates].[UserId]
                                                                                          AND [Exists].[GuildId] = @guildId
                                                                                          AND [Exists].[Type] = 0
                                                                                          AND [Exists].[Points] > -0.29 )
                                                                             THEN COALESCE ( ( SUM ( [WeightedUserPoints]  ) / CAST ( 10.7502502884829 AS FLOAT ) * CAST ( 0.29 AS FLOAT ) ), 0 )
                                                                         ELSE 0
                                                                     END ) AS [Points]
                                                               FROM ( SELECT [Summed].[UserId],
                                                                             [Summed].[Date],
                                                                             [Summed].[Week],
                                                                             ( CASE 
                                                                                 WHEN [Summed].[Value] <= 0
                                                                                     THEN CAST ( 0.0000000000000000 AS FLOAT )
                                                                                 WHEN [Summed].[Value]  > 0 AND [Summed].[Value] < [WeeklyReference].[Value]
                                                                                     THEN CAST ( 0.0414285714285714 AS FLOAT )
                                                                                 WHEN [Summed].[Value] / [WeeklyReference].[Value] >= 6
                                                                                     THEN CAST ( 0.2900000000000000 AS FLOAT )
                                                                                 ELSE ( [Summed].[Value] / [WeeklyReference].[Value] * CAST ( 0.0414285714285714 AS FLOAT ) ) + CAST ( 0.0414285714285714 AS FLOAT )
                                                                             END ) * [WeeklyReference].[Weight] AS [WeightedUserPoints],
                                                                             [WeeklyReference].[Weight] * CAST ( 0.29 AS FLOAT ) AS [WeightedMaximumPoints]
                                                                        FROM ( SELECT [RawDay].[UserId],
                                                                                      [RawDay].[Date],
                                                                                      [RawDay].[Week],
                                                                                      SUM ( CASE
                                                                                                 WHEN ( [RawDay].[IsThresholdRelevant] = 1 AND [RawDay].[RowNumber] > 3 ) THEN [RawDay].[Value] / 5
                                                                                                 ELSE [RawDay].[Value]
                                                                                            END ) AS [Value]
                                                                                 FROM ( SELECT [Raw].[UserId],
                                                                                               [Raw].[Date],
                                                                                               [Raw].[Week],
                                                                                               [Raw].[DonationDate],
                                                                                               [Raw].[Value],
                                                                                               [Raw].[IsThresholdRelevant],
                                                                                               ROW_NUMBER() OVER ( PARTITION BY [Raw].[UserId], [Raw].[Date], [Raw].[DonationDate]
                                                                                                                       ORDER BY [Raw].[UserId], [Raw].[Date], [Raw].[DonationDate], [Raw].[IsThresholdRelevant] DESC ) AS [RowNumber]
                                                                                          FROM ( SELECT [CurrentPoints].[UserId],
                                                                                                        [CurrentPoints].[Date],
                                                                                                        ( DATEDIFF ( DAY, [LogEntry].[Time], [CurrentPoints].[Date] ) / 7) + 1  AS [Week],
                                                                                                        DATEADD(dd, 0, DATEDIFF(dd, 0, [LogEntry].[Time])) AS [DonationDate],
                                                                                                        [Donation].[Value],
                                                                                                        [Donation].[IsThresholdRelevant]
                                                                                                   FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                                             INNER JOIN [GuildWarsAccounts] AS [Account]
                                                                                                     ON [Account].[UserId] = [CurrentPoints].[UserId]
                                                                                             INNER JOIN [GuildLogEntries] AS [LogEntry]
                                                                                                     ON [LogEntry].[GuildId] = @guildId
                                                                                                    AND [LogEntry].[User] = [Account].[Name]
                                                                                                    AND [LogEntry].[Time] > DATEADD ( DAY, -63, [CurrentPoints].[Date] )   
                                                                                                    AND [LogEntry].[Time] < DATEADD ( DAY,   1, [CurrentPoints].[Date] )
                                                                                             INNER JOIN [GuildDonations] AS [Donation]
                                                                                                     ON [Donation].[GuildId] = @guildId
                                                                                                    AND [Donation].[LogEntryId] = [LogEntry].[Id]
                                                                                                  WHERE [CurrentPoints].[Type] = 0       
                                                                                                    AND [CurrentPoints].[Date] >= @from
                                                                                                    AND [CurrentPoints].[Date] <= @to
                                                                                                    AND [CurrentPoints].[GuildId] = @guildId ) AS [Raw] ) AS [RawDay]
                                                                             GROUP BY [RawDay].[UserId],
                                                                                      [RawDay].[Date],
                                                                                      [RawDay].[Week] ) AS [Summed]
                                                                 
                                                                 CROSS APPLY ScruffyGetWeeklyDonationReferences ( @guildId,
                                                                                                                  DATEADD ( DAY, -63, [Summed].[Date] ),
                                                                                                                  DATEADD ( DAY,   1, [Summed].[Date] ) ) AS [WeeklyReference]
                                                                       WHERE [WeeklyReference].[Week] = [Summed].[Week] ) AS [Dates]
                                                           GROUP BY [Dates].[UserId],
                                                                    [Dates].[Date] 
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentDonationPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 4
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 4, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Donations", null, dbFactory.LastError);
                }

                // Discord voice activity
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentVoiceActivityPoints]
                                                         AS
                                                         (
                                                             SELECT [Dates].[Date],
                                                                    [Dates].[UserId], 
                                                                    CASE 
                                                                        WHEN EXISTS ( SELECT 1 
                                                                                        FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                       WHERE [Exists].[Date] = [Dates].[Date]
                                                                                         AND [Exists].[UserId] = [Dates].[UserId]
                                                                                         AND [Exists].[GuildId] = @guildId
                                                                                         AND [Exists].[Type] = 0
                                                                                         AND [Exists].[Points] > -0.29 )
                                                                            THEN COALESCE ( ( [Points] ), 0)
                                                                        ELSE 0
                                                                    END AS [Points]
                                                             FROM ( SELECT [CurrentPoints].[UserId], 
                                                                           [CurrentPoints].[Date],
                                                                           MAX ( COALESCE ( [Points].[Points], 0 ) ) AS [Points]
                                                                      FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                INNER JOIN [DiscordAccounts] AS [Account]
                                                                        ON [Account].[UserId] = [CurrentPoints].[UserId]
                                                                 LEFT JOIN [DiscordHistoricAccountRoleAssignments] AS [Role]
                                                                        ON [Role].[AccountId] = [Account].[Id]
                                                                       AND [Role].[Date] = [CurrentPoints].[Date]
                                                                 LEFT JOIN [GuildDiscordActivityPointsAssignments] AS [Points]
                                                                        ON [Points].[RoleId] = [Role].[RoleId]
                                                                       AND [Points].[Type] = 0
                                                                 LEFT JOIN [Guilds] AS [Guild]
                                                                        ON [Guild].[DiscordServerId] = [Role].[ServerId]
                                                                     WHERE [CurrentPoints].[Type] = 0       
                                                                       AND [CurrentPoints].[Date] >= @from
                                                                       AND [CurrentPoints].[Date] <= @to
                                                                       AND [CurrentPoints].[GuildId] = @guildId
                                                                 GROUP BY [CurrentPoints].[UserId],
                                                                          [CurrentPoints].[Date] ) AS [Dates]
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentVoiceActivityPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 5
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 5, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Discord voice activity points", null, dbFactory.LastError);
                }

                // Discord message activity
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentVoiceActivityPoints]
                                                         AS
                                                         (
                                                             SELECT [Dates].[Date],
                                                                    [Dates].[UserId], 
                                                                    CASE 
                                                                        WHEN EXISTS ( SELECT 1 
                                                                                        FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                       WHERE [Exists].[Date] = [Dates].[Date]
                                                                                         AND [Exists].[UserId] = [Dates].[UserId]
                                                                                         AND [Exists].[GuildId] = @guildId
                                                                                         AND [Exists].[Type] = 0
                                                                                         AND [Exists].[Points] > -0.29 )
                                                                            THEN COALESCE ( ( [Points] ), 0)
                                                                        ELSE 0
                                                                    END AS [Points]
                                                             FROM ( SELECT [CurrentPoints].[UserId], 
                                                                           [CurrentPoints].[Date],
                                                                           MAX ( COALESCE ( [Points].[Points], 0 ) ) AS [Points]
                                                                      FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                INNER JOIN [DiscordAccounts] AS [Account]
                                                                        ON [Account].[UserId] = [CurrentPoints].[UserId]
                                                                 LEFT JOIN [DiscordHistoricAccountRoleAssignments] AS [Role]
                                                                        ON [Role].[AccountId] = [Account].[Id]
                                                                       AND [Role].[Date] = [CurrentPoints].[Date]
                                                                 LEFT JOIN [GuildDiscordActivityPointsAssignments] AS [Points]
                                                                        ON [Points].[RoleId] = [Role].[RoleId]
                                                                       AND [Points].[Type] = 1
                                                                 LEFT JOIN [Guilds] AS [Guild]
                                                                        ON [Guild].[DiscordServerId] = [Role].[ServerId]
                                                                     WHERE [CurrentPoints].[Type] = 0       
                                                                       AND [CurrentPoints].[Date] >= @from
                                                                       AND [CurrentPoints].[Date] <= @to
                                                                       AND [CurrentPoints].[GuildId] = @guildId
                                                                 GROUP BY [CurrentPoints].[UserId],
                                                                          [CurrentPoints].[Date] ) AS [Dates]
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentVoiceActivityPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 6
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 6, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Discord message activity points", null, dbFactory.LastError);
                }

                // Events
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentEventActivityPoints]
                                                         AS
                                                         (
                                                              SELECT [Dates].[UserId], 
                                                                     [Dates].[Date],
                                                                     ( CASE 
                                                                           WHEN EXISTS ( SELECT 1 
                                                                                           FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                          WHERE [Exists].[Date] = [Dates].[Date]
                                                                                            AND [Exists].[UserId] = [Dates].[UserId]
                                                                                            AND [Exists].[GuildId] = @guildId
                                                                                            AND [Exists].[Type] = 0
                                                                                            AND [Exists].[Points] > -0.29 )
                                                                               THEN COALESCE ( ( SUM ( [WeightedUserPoints]  ) / SUM ( [WeightedMaximumPoints] ) * AVG ( [MaximumPoints] ) ), 0 )
                                                                           ELSE 0
                                                                       END ) / 7 AS [Points]
                                                                FROM ( SELECT [Summed].[UserId],
                                                                              [Summed].[Date],
                                                                              [Summed].[Week],
                                                                              ( CASE 
                                                                                    WHEN ( [Summed].[GuildPoints] > [WeeklyMaximum].[Points] )
                                                                                        THEN [WeeklyMaximum].[Points] * [WeeklyMaximum].[Weight]
                                                                                    ELSE [Summed].[GuildPoints] * [WeeklyMaximum].[Weight]
                                                                                END
                                                                              + [Summed].[LeaderPoints] * [WeeklyMaximum].[Weight] ) AS [WeightedUserPoints],
                                                                              [WeeklyMaximum].[Weight] * [WeeklyMaximum].[Points] AS [WeightedMaximumPoints],
                                                                              [WeeklyMaximum].[Points] AS [MaximumPoints]
                                                                         FROM ( SELECT [Raw].[UserId],
                                                                                       [Raw].[Date],
                                                                                       [Raw].[Week],
                                                                                       SUM ( [Raw].[GuildPoints] ) AS [GuildPoints],
                                                                                       SUM ( [Raw].[LeaderPoints] ) AS [LeaderPoints]
                                                                                  FROM ( SELECT [CurrentPoints].[UserId],
                                                                                                [CurrentPoints].[Date],
                                                                                                ( DATEDIFF ( DAY, [Appointment].[TimeStamp], [CurrentPoints].[Date] ) / 7) + 1  AS [Week],
                                                                                                [Template].[GuildPoints],
                                                                                                CAST ( [Participant].[IsLeader] AS FLOAT ) AS [LeaderPoints]
                                                                                          FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                                    INNER JOIN [CalendarAppointments] AS [Appointment]
                                                                                            ON [Appointment].[TimeStamp] > DATEADD ( DAY, -63, [CurrentPoints].[Date] )   
                                                                                           AND [Appointment].[TimeStamp] < DATEADD ( DAY,   1, [CurrentPoints].[Date] )   
                                                                                     LEFT JOIN [CalendarAppointmentParticipants] AS [Participant]
                                                                                            ON [Participant].[UserId] = [CurrentPoints].[UserId]
                                                                                           AND [Participant].[AppointmentId] = [Appointment].[Id]
                                                                                    INNER JOIN [CalendarAppointmentTemplates] AS [Template]
                                                                                            ON [Template].[Id]  = [Appointment].[CalendarAppointmentTemplateId]
                                                                                         WHERE [CurrentPoints].[Type] = 0       
                                                                                           AND [CurrentPoints].[Date] >= @from
                                                                                           AND [CurrentPoints].[Date] <= @to
                                                                                           AND [CurrentPoints].[GuildId] = @guildId ) AS [Raw]
                                                                               GROUP BY [Raw].[UserId], 
                                                                                        [Raw].[Date], 
                                                                                        [Raw].[Week] ) AS [Summed]
                                                                  CROSS APPLY ScruffyGetWeeklyEventPoints ( DATEADD ( DAY, -63, [Summed].[Date] ),
                                                                                                            DATEADD ( DAY,   1, [Summed].[Date] ) ) AS [WeeklyMaximum]
                                                                        WHERE [WeeklyMaximum].[Week] = [Summed].[Week] ) AS [Dates]
                                                            GROUP BY [Dates].[UserId],
                                                                     [Dates].[Date]
                                                         )
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentEventActivityPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 7
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 7, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Event points", null, dbFactory.LastError);
                }

                // Development
                if (await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentCommitPoints]
                                                         AS
                                                         (
                                                             SELECT [Dates].[UserId], 
                                                                    [Dates].[Date],
                                                                    ( CASE 
                                                                            WHEN EXISTS ( SELECT 1 
                                                                                            FROM [GuildRankCurrentPoints] AS [Exists]
                                                                                        WHERE [Exists].[Date] = [Dates].[Date]
                                                                                            AND [Exists].[UserId] = [Dates].[UserId]
                                                                                            AND [Exists].[GuildId] = @guildId
                                                                                            AND [Exists].[Type] = 0
                                                                                            AND [Exists].[Points] > -0.29 )
                                                                                THEN COALESCE ( ( SUM ( [WeightedUserPoints]  ) / CAST ( 10.7502502884829 AS FLOAT ) * CAST ( 0.29 AS FLOAT ) ), 0 )
                                                                            ELSE 0
                                                                        END ) AS [Points]
                                                               FROM ( SELECT [Summed].[UserId],
                                                                             [Summed].[Date],
                                                                             [Summed].[Week],
                                                                             ( CASE 
                                                                                 WHEN [Summed].[Count] <= 0
                                                                                     THEN CAST ( 0.0000000000000000 AS FLOAT )
                                                                                 WHEN [Summed].[Count]  > 0 AND [Summed].[Count] < [WeeklyReference].[Count]
                                                                                     THEN CAST ( 0.0414285714285714 AS FLOAT )
                                                                                 WHEN [Summed].[Count] / [WeeklyReference].[Count] >= 6
                                                                                     THEN CAST ( 0.2900000000000000 AS FLOAT )
                                                                                 ELSE ( [Summed].[Count] / [WeeklyReference].[Count] * CAST ( 0.0414285714285714 AS FLOAT ) ) + CAST ( 0.0414285714285714 AS FLOAT )
                                                                             END ) * [WeeklyReference].[Weight] AS [WeightedUserPoints]
                                                                        FROM (  SELECT [Raw].[UserId],
                                                                                       [Raw].[Date],
                                                                                       [Raw].[Week],
                                                                                       COUNT (*) AS [Count]
                                                                                  FROM ( SELECT [CurrentPoints].[UserId],
                                                                                                [CurrentPoints].[Date],
                                                                                                ( DATEDIFF ( DAY, [Commit].[TimeStamp], [CurrentPoints].[Date] ) / 7) + 1  AS [Week],
                                                                                                DATEADD(dd, 0, DATEDIFF(dd, 0, [Commit].[TimeStamp])) AS [DonationDate]
                                                                                            FROM [GuildRankCurrentPoints] AS [CurrentPoints]
                                                                                      INNER JOIN [Users] AS [User]
                                                                                              ON [User].[Id] = [CurrentPoints].[UserId]
                                                                                      INNER JOIN [GitHubCommits] AS [Commit]
                                                                                              ON [Commit].[Author] = [User].[GitHubAccount]
                                                                                             AND [Commit].[TimeStamp] > DATEADD ( DAY, -63, [CurrentPoints].[Date] )   
                                                                                             AND [Commit].[TimeStamp] < DATEADD ( DAY,   1, [CurrentPoints].[Date] )
                                                                                           WHERE [CurrentPoints].[Type] = 0       
                                                                                             AND [CurrentPoints].[Date] >= @from
                                                                                             AND [CurrentPoints].[Date] <= @to
                                                                                             AND [CurrentPoints].[GuildId] = @guildId ) AS [Raw] 
                                                                                GROUP BY [Raw].[UserId],
                                                                                         [Raw].[Date],
                                                                                         [Raw].[Week] ) AS [Summed]
                                                                                                             
                                                                 CROSS APPLY ScruffyGetWeeklyCommits ( @guildId,
                                                                                                       DATEADD ( DAY, -63, [Summed].[Date] ),
                                                                                                       DATEADD ( DAY,   1, [Summed].[Date] ) ) AS [WeeklyReference]
                                                                       WHERE [WeeklyReference].[Week] = [Summed].[Week] ) AS [Dates]
                                                           GROUP BY [Dates].[UserId],
                                                                    [Dates].[Date] 
                                                         )
                                                         
                                                         
                                                         MERGE INTO [GuildRankCurrentPoints] AS [Target]
                                                              USING [CurrentCommitPoints] AS [SOURCE]
                                                         
                                                                 ON [Target].[GuildId] = @guildId
                                                                AND [Target].[UserId] = [Source].[UserId]
                                                                AND [Target].[Date] = [Source].[Date]
                                                                AND [Target].[Type] = 8
                                                         
                                                         WHEN MATCHED 
                                                           THEN UPDATE
                                                                SET [Target].[Points] = [Source].[Points]
                                                         
                                                         WHEN NOT MATCHED
                                                            THEN INSERT ( [GuildId], [UserId], [Date], [Type], [Points] )
                                                                 VALUES ( @guildId, [Source].[UserId], [Source].[Date], 8, [Source].[Points] );",
                                                       new SqlParameter("@from", from),
                                                       new SqlParameter("@to", to),
                                                       new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false)
                 == null)
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildRankService), "Development points", null, dbFactory.LastError);
                }
            }
        }
    }

    /// <summary>
    /// Post assignment overview
    /// </summary>
    /// <param name="guildId">Guild id</param>
    /// <param name="isLiveDetermination">Is live rank determination?</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<Embed> CheckCurrentAssignments(long guildId, bool isLiveDetermination)
    {
        Dictionary<string, string> currentRanks;

        if (isLiveDetermination)
        {
            var guild = _repositoryFactory.GetRepository<GuildRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.Id == guildId)
                                          .Select(obj => new
                                                         {
                                                             obj.GuildId,
                                                             obj.ApiKey
                                                         })
                                          .First();

            var connector = new GuildWars2ApiConnector(guild.ApiKey);
            await using (connector.ConfigureAwait(false))
            {
                var members = await connector.GetGuildMembers(guild.GuildId)
                                             .ConfigureAwait(false);

                currentRanks = members.ToDictionary(obj => obj.Name, obj => obj.Rank);
            }
        }
        else
        {
            var today = DateTime.Today;

            currentRanks = _repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                              .GetQuery()
                                              .Where(obj => obj.Date == today
                                                         && obj.GuildId == guildId)
                                              .ToDictionary(obj => obj.Name, obj => obj.Rank);
        }

        var guildWarsAccounts = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                  .GetQuery()
                                                  .Select(obj => obj);

        var assignedRanks = _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                              .GetQuery()
                                              .Where(obj => obj.GuildId == guildId)
                                              .Join(guildWarsAccounts,
                                                    obj => obj.UserId,
                                                    obj => obj.UserId,
                                                    (obj1, obj2) => new
                                                                    {
                                                                        obj2.Name,
                                                                        obj1.Rank
                                                                    })
                                              .ToDictionary(obj => obj.Name, obj => obj.Rank);

        var ranks = _repositoryFactory.GetRepository<GuildRankRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.GuildId == guildId)
                                      .Select(obj => new
                                                     {
                                                         obj.InGameName,
                                                         obj.Order
                                                     });

        var invalidRanks = new List<(string Rank, string Name, int Order)>();

        foreach (var currentRank in currentRanks)
        {
            if (assignedRanks.TryGetValue(currentRank.Key, out var assignedRank))
            {
                if (currentRank.Value != assignedRank.InGameName)
                {
                    var rank = ranks.First(obj => obj.InGameName == assignedRank.InGameName);

                    invalidRanks.Add((rank.InGameName, currentRank.Key, rank.Order));
                }
            }
            else
            {
                var rank = ranks.OrderByDescending(obj => obj.Order)
                                .First();

                if (currentRank.Value != rank.InGameName)
                {
                    invalidRanks.Add((rank.InGameName, currentRank.Key, rank.Order));
                }
            }
        }

        if (invalidRanks.Any())
        {
            var embed = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("RequiredRankChangesTitle", "Required rank changes"))
                                          .WithDescription(LocalizationGroup.GetText("RequiredRankChangesDescription", "The following ranks have to be changed in game:"))
                                          .WithColor(Color.Green)
                                          .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                          .WithTimestamp(DateTime.Now);

            foreach (var rank in invalidRanks.OrderBy(obj => obj.Order).GroupBy(obj => obj.Rank))
            {
                var stringBuilder = new StringBuilder();

                foreach (var user in rank.OrderBy(obj => obj.Name))
                {
                    stringBuilder.AppendLine(user.Name);
                }

                embed.AddField(rank.Key, stringBuilder.ToString());
            }

            return embed.Build();
        }

        return null;
    }

    #endregion // Methods
}