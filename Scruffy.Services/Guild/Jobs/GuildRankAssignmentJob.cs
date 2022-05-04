using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Rank assignment job
/// </summary>
public class GuildRankAssignmentJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repositoryFactory">Repository factory</param>
    public GuildRankAssignmentJob(RepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task ExecuteOverrideAsync()
    {
        var today = DateTime.Today;
        var limit = DateTime.Today.AddDays(-63);

        foreach (var guildId in _repositoryFactory.GetRepository<GuildRepository>()
                                                  .GetQuery()
                                                  .Select(obj => obj.Id)
                                                  .ToList())
        {
            var userConfiguration = _repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                                                      .GetQuery()
                                                      .Select(obj => obj);

            var guildMemberQuery = _repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                     .GetQuery()
                                                     .Select(obj => obj);

            var accountsQuery = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                  .GetQuery()
                                                  .Select(obj => obj);

            var rankAssignments = _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                                    .GetQuery()
                                                    .Select(obj => obj);

            var users = _repositoryFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.Date >= limit
                                                     && obj.Date < today
                                                     && obj.GuildId == guildId
                                                     && accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                               && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                                            && obj3.GuildId == obj.GuildId
                                                                                                            && obj3.Date == today))
                                                     && userConfiguration.Any(obj2 => obj2.UserId == obj.UserId
                                                                                   && obj2.GuildId == guildId
                                                                                   && obj2.IsFixedRank) == false)
                                          .GroupBy(obj => obj.UserId)
                                          .Select(obj => new
                                                         {
                                                             UserId = obj.Key,
                                                             Points = obj.Sum(obj2 => obj2.Points),
                                                             CurrentRankId = rankAssignments.Where(obj2 => obj2.GuildId == guildId
                                                                                                        && obj2.UserId == obj.Key)
                                                                                            .Select(obj2 => (int?)obj2.RankId)
                                                                                            .FirstOrDefault(),
                                                             CurrentRankAssignment = rankAssignments.Where(obj2 => obj2.GuildId == guildId
                                                                                                                && obj2.UserId == obj.Key)
                                                                                                    .Select(obj2 => (DateTime?)obj2.TimeStamp)
                                                                                                    .FirstOrDefault()
                                                         })
                                          .OrderByDescending(obj => obj.Points)
                                          .ToList();

            var ranks = _repositoryFactory.GetRepository<GuildRankRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.GuildId == guildId)
                                          .Select(obj => new
                                                         {
                                                             obj.Id,
                                                             obj.Order,
                                                             obj.Percentage,
                                                         })
                                          .OrderByDescending(obj => obj.Order)
                                          .ToList();

            var ranksStack = new Stack<GuildRankAssignmentData>(ranks.Select(obj => new GuildRankAssignmentData
                                                                                    {
                                                                                        RankId = obj.Id,
                                                                                        Order = obj.Order,
                                                                                        Slots = (int)Math.Round(obj.Percentage * users.Count, MidpointRounding.AwayFromZero)
                                                                                    }));

            var currentAssignment = ranksStack.Peek();

            foreach (var user in users)
            {
                if (currentAssignment.Slots <= 0
                 && ranks.Count > 0)
                {
                    currentAssignment = ranksStack.Pop();
                    currentAssignment = ranksStack.Peek();
                }

                if (user.CurrentRankId != currentAssignment.RankId
                 && (user.CurrentRankAssignment == null
                  || DateTime.Now - user.CurrentRankAssignment > TimeSpan.FromDays(7)))
                {
                    _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                      .AddOrRefresh(obj => obj.UserId == user.UserId
                                                        && obj.GuildId == guildId,
                                                    obj =>
                                                    {
                                                        obj.GuildId = guildId;
                                                        obj.UserId = user.UserId;

                                                        var oldRank = ranks.FirstOrDefault(obj2 => obj2.Id == user.CurrentRankId);
                                                        if (oldRank == null)
                                                        {
                                                            obj.RankId = ranks.OrderByDescending(obj2 => obj2.Order)
                                                                              .Select(obj2 => obj2.Id)
                                                                              .FirstOrDefault();
                                                        }
                                                        else if (Math.Abs(oldRank.Order - currentAssignment.Order) > 1)
                                                        {
                                                            var nextOrder = currentAssignment.Order > oldRank.Order
                                                                                ? oldRank.Order + 1
                                                                                : oldRank.Order - 1;

                                                            obj.RankId = ranks.Where(obj2 => obj2.Order == nextOrder)
                                                                              .Select(obj2 => obj2.Id)
                                                                              .FirstOrDefault();
                                                        }
                                                        else
                                                        {
                                                            obj.RankId = currentAssignment.RankId;
                                                        }

                                                        obj.TimeStamp = DateTime.Now;
                                                    });
                }

                --currentAssignment.Slots;
            }

            var loginQuery = _repositoryFactory.GetRepository<GuildWarsAccountDailyLoginCheckRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

            var rankAssignmentQuery = _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                                        .GetQuery()
                                                        .Select(obj => obj);

            var lastRankId = ranks.OrderByDescending(obj2 => obj2.Order)
                                  .Select(obj2 => obj2.Id)
                                  .Skip(1)
                                  .FirstOrDefault();

            var inactivityLimit = DateTime.Today.AddMonths(-1);
            var assignmentLimit = DateTime.Now.AddDays(-7);

            foreach (var userId in _repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                   .GetQuery()
                                                   .Where(obj => obj.GuildId == guildId
                                                              && obj.Date == today)
                                                   .Join(accountsQuery,
                                                         obj => obj.Name,
                                                         obj => obj.Name,
                                                         (obj1, obj2) => new
                                                                         {
                                                                             obj2.UserId,
                                                                             obj2.Name
                                                                         })
                                                   .Join(loginQuery.DefaultIfEmpty(),
                                                         obj => obj.Name,
                                                         obj => obj.Name,
                                                         (obj1, obj2) => new
                                                                         {
                                                                             obj1.UserId,
                                                                             LastLogin = (DateTime?)obj2.Date
                                                                         })
                                                   .Join(rankAssignmentQuery,
                                                         obj => obj.UserId,
                                                         obj => obj.UserId,
                                                         (obj1, obj2) => new
                                                                         {
                                                                             obj1.UserId,
                                                                             obj1.LastLogin,
                                                                             obj2.RankId,
                                                                             obj2.TimeStamp
                                                                         })
                                                   .GroupBy(obj => obj.UserId)
                                                   .Select(obj => new
                                                                  {
                                                                      UserId = obj.Key,
                                                                      RankId = obj.Max(obj2 => obj2.RankId),
                                                                      AssignmentTimeStamp = obj.Max(obj2 => obj2.TimeStamp),
                                                                      LastLogin = obj.Max(obj2 => obj2.LastLogin)
                                                                  })
                                                   .Where(obj => obj.RankId == lastRankId
                                                              && obj.LastLogin < inactivityLimit
                                                              && assignmentLimit > obj.AssignmentTimeStamp
                                                              && userConfiguration.Any(obj2 => obj2.UserId == obj.UserId
                                                                                            && obj2.GuildId == guildId
                                                                                            && obj2.IsInactive) == false)
                                                   .Select(obj => obj.UserId)
                                                   .ToList())
            {
                _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                  .AddOrRefresh(obj => obj.UserId == userId
                                                    && obj.GuildId == guildId,
                                                obj =>
                                                {
                                                    obj.GuildId = guildId;
                                                    obj.UserId = userId;
                                                    obj.RankId = ranks.OrderByDescending(obj2 => obj2.Order)
                                                                      .Select(obj2 => obj2.Id)
                                                                      .FirstOrDefault();
                                                    obj.TimeStamp = DateTime.Now;
                                                });
            }

            _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                              .RemoveRange(obj => accountsQuery.Any(obj2 => obj2.UserId == obj.UserId
                                                                         && guildMemberQuery.Any(obj3 => obj3.Name == obj2.Name
                                                                                                      && obj3.GuildId == obj.GuildId
                                                                                                      && obj3.Date == today)) == false);
        }

        return Task.CompletedTask;
    }

    #endregion // LocatedAsyncJob
}