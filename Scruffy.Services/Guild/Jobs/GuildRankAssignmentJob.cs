using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs
{
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
                                                                                            Slots = (int)Math.Round(obj.Percentage * users.Count)
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
            }

            return Task.CompletedTask;
        }

        #endregion // LocatedAsyncJob
    }
}
