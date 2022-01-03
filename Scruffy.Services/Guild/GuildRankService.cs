using DSharpPlus;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild
{
    /// <summary>
    /// Guild rank management
    /// </summary>
    public class GuildRankService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Discord-Client
        /// </summary>
        private readonly DiscordClient _discordClient;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="discordClient">Discord-Client</param>
        public GuildRankService(LocalizationService localizationService, DiscordClient discordClient)
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
                        var discordServer = await _discordClient.GetGuildAsync(guild.DiscordServerId)
                                                                .ConfigureAwait(false);

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
                            var member = await discordServer.GetMemberAsync(user.DiscordUserId)
                                                            .ConfigureAwait(false);

                            foreach (var role in member.Roles.ToList())
                            {
                                if (user.DiscordRoleId != role.Id
                                 && guild.Roles.Contains(role.Id))
                                {
                                    await member.RevokeRoleAsync(role)
                                                .ConfigureAwait(false);
                                }
                            }

                            if (member.Roles.Any(obj => obj.Id == user.DiscordRoleId) == false)
                            {
                                await member.GrantRoleAsync(discordServer.GetRole(user.DiscordRoleId))
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

                    var discordServer = await _discordClient.GetGuildAsync(guild.DiscordServerId)
                                                            .ConfigureAwait(false);

                    foreach (var member in await discordServer.GetAllMembersAsync()
                                                              .ConfigureAwait(false))
                    {
                        var assignedRoleId = default(ulong?);

                        if (users.TryGetValue(member.Id, out var discordRoleId))
                        {
                            assignedRoleId = discordRoleId;
                        }

                        foreach (var role in member.Roles.ToList())
                        {
                            if (assignedRoleId != role.Id
                            && guild.Roles.Contains(role.Id))
                            {
                                await member.RevokeRoleAsync(role)
                                            .ConfigureAwait(false);
                            }
                        }

                        if (assignedRoleId != null
                            && member.Roles.Any(obj => obj.Id == assignedRoleId) == false)
                        {
                            await member.GrantRoleAsync(discordServer.GetRole(assignedRoleId.Value))
                                        .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        #endregion // Methods
    }
}
