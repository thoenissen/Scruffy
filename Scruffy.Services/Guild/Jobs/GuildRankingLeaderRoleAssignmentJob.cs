using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Assignment of the ranking leader role
/// </summary>
public class GuildRankingLeaderRoleAssignmentJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordSocketClient _discordClient;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="discordClient">Discord client</param>
    public GuildRankingLeaderRoleAssignmentJob(RepositoryFactory repositoryFactory, DiscordSocketClient discordClient)
    {
        _repositoryFactory = repositoryFactory;
        _discordClient = discordClient;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Role assignment
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="discordServerId">Discord server ID</param>
    /// <param name="rankingLeaderRoleId">Ranking leader role ID</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task AssignRole(long guildId, ulong discordServerId, ulong rankingLeaderRoleId)
    {
        var discordGuild = _discordClient.GetGuild(discordServerId);
        var discordRole = discordGuild.GetRole(rankingLeaderRoleId);

        if (discordRole == null)
        {
            return;
        }

        var today = DateTime.Today;
        var limit = DateTime.Today.AddDays(-63);
        var guildMemberQuery = _repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                 .GetQuery()
                                                 .Select(obj => obj);

        var accountsQuery = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                              .GetQuery()
                                              .Select(obj => obj);

        var leadingUser = _repositoryFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.Date >= limit
                                                          && obj.Date < today
                                                          && obj.GuildId == guildId
                                                          && accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                                       && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                           && obj3.GuildId == obj.GuildId
                                                                                           && obj3.Date == today)))
                                            .GroupBy(obj => obj.UserId)
                                            .Select(obj => new
                                                           {
                                                               UserId = obj.Key,
                                                               Points = obj.Sum(obj2 => obj2.Points),
                                                           })
                                            .OrderByDescending(obj => obj.Points)
                                            .FirstOrDefault()
                                            ?.UserId;

        var rankQuery = _repositoryFactory.GetRepository<GuildRankRepository>().GetQuery();

        if (leadingUser != null
            && _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                 .GetQuery()
                                 .Any(rankAssignment => rankAssignment.UserId == leadingUser
                                                        && rankAssignment.RankId == rankQuery.OrderBy(rank => rank.Order)
                                                                                             .Select(rank => rank.Id)
                                                                                             .FirstOrDefault()))
        {
            leadingUser = null;
        }

        var roleAssignments = discordRole.Members.ToList();

        List<ulong> discordAccountIds = [];

        if (leadingUser != null)
        {
            discordAccountIds = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                  .GetQuery()
                                                  .Where(discordAccount => discordAccount.UserId == leadingUser.Value)
                                                  .Select(discordAccount => discordAccount.Id)
                                                  .ToList();
        }

        foreach (var discordAccountId in discordAccountIds)
        {
            var member = discordGuild.GetUser(discordAccountId);

            if (member?.Roles.Any(role => role.Id == rankingLeaderRoleId) == false)
            {
                await member.AddRoleAsync(discordRole).ConfigureAwait(false);
            }
        }

        foreach (var roleAssignment in roleAssignments)
        {
            if (discordAccountIds.Contains(roleAssignment.Id) == false)
            {
                await roleAssignment.RemoveRoleAsync(discordRole).ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods

    #region LocatedAsyncJob

    /// <inheritdoc />
    public override async Task ExecuteOverrideAsync()
    {
        foreach (var guild in _repositoryFactory.GetRepository<GuildRepository>()
                                                .GetQuery()
                                                .Where(guild => guild.RankingLeaderRoleId != null)
                                                .Select(obj => new
                                                               {
                                                                   obj.Id,
                                                                   obj.DiscordServerId,
                                                                   obj.RankingLeaderRoleId
                                                               })
                                                .ToList())
        {
            await AssignRole(guild.Id, guild.DiscordServerId, guild.RankingLeaderRoleId!.Value).ConfigureAwait(false);
        }
    }

    #endregion // LocatedAsyncJob
}