﻿using Discord.WebSocket;

using Microsoft.Data.SqlClient;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="discordClient">Discord-Client</param>
    public GuildRankService(LocalizationService localizationService, DiscordSocketClient discordClient)
        : base(localizationService)
    {
        _discordClient = discordClient;
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
                var connector = new GuidWars2ApiConnector(guild.ApiKey);
                await using (connector.ConfigureAwait(false))
                {
                    var members = await connector.GetGuildMembers(guild.GuildId)
                                                 .ConfigureAwait(false);

                    await dbFactory.GetRepository<GuildWarsGuildMemberRepository>()
                                   .BulkInsert(guild.Id, members.Select(obj => (obj.Name, obj.Rank)))
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
    public async Task RefreshDiscordRank(long guildId, string accountName, string rankName)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var isChanged = false;

            dbFactory.GetRepository<GuildWarsGuildMemberRepository>()
                     .AddOrRefresh(obj => obj.GuildId == guildId
                                       && obj.Name == accountName,
                                   obj =>
                                   {
                                       obj.GuildId = guildId;
                                       obj.Name = accountName;

                                       isChanged = obj.Rank != rankName;

                                       obj.Rank = rankName;
                                   });

            if (isChanged)
            {
                var guildMemberQuery = dbFactory.GetRepository<GuildWarsGuildMemberRepository>()
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
                                                     .Join(guildMemberQuery,
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
            var guildMemberQuery = dbFactory.GetRepository<GuildWarsGuildMemberRepository>()
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
                var users = guildMemberQuery.Where(obj => obj.GuildId == guild.Id)
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
                    foreach (var member in members)
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
                await dbFactory.ExecuteSqlRawAsync(@"WITH [CurrentPoints]
                                                         AS
                                                         (
                                                             SELECT [DateRange].[Value] AS [Date],
                                                                    [GuildWarsAccount].[UserId], 
                                                                    CASE
                                                                        WHEN MAX([DailyLogin].[Date]) IS NULL THEN -0.7
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 28 THEN -0.57
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 21 THEN -0.43
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 14 THEN -0.29
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 7 THEN -0.11
                                                                        WHEN DATEDIFF(day, MAX([DailyLogin].[Date]), [DateRange].[Value]) > 3 THEN 0.11
                                                                        ELSE 0.18
                                                                    END AS [Points]
                                                         
                                                               FROM ( SELECT [Value] FROM [GetDateRange](@from, @to)) AS [DateRange]
                                                         
                                                          LEFT JOIN [GuildWarsGuildMembers] AS [Member]
                                                                 ON 1 = 1
                                                         INNER JOIN [GuildWarsAccounts] AS [GuildWarsAccount]
                                                                 ON [Member].[Name] = [GuildWarsAccount].[Name]
                                                          LEFT JOIN [GuildWarsAccountDailyLoginChecks] AS [DailyLogin]
                                                                 ON [Member].[Name] = [DailyLogin].[Name]
                                                         
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
                                                   new SqlParameter("@from", DateTime.Today.AddDays(-61)),
                                                   new SqlParameter("@to", DateTime.Today.AddDays(-1)),
                                                   new SqlParameter("@guildId", guild.Id))
                               .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}