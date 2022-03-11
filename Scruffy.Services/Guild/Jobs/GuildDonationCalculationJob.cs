using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs
{
    /// <summary>
    /// Calculation of the guild donations
    /// </summary>
    internal class GuildDonationCalculationJob : LocatedAsyncJob
    {
        /// <summary>
        /// Repository factory
        /// </summary>
        private readonly RepositoryFactory _dbFactory;

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbFactory">Repository factory</param>
        public GuildDonationCalculationJob(RepositoryFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        #endregion // Constructor

        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteOverrideAsync()
        {
            var accounts = _dbFactory.GetRepository<GuildWarsAccountRepository>()
                                     .GetQuery()
                                     .Select(obj => obj);

            // Stash - Coins
            foreach (var logEntry in await _dbFactory.GetRepository<GuildLogEntryRepository>()
                                                     .GetQuery()
                                                     .Join(accounts,
                                                           obj => obj.User,
                                                           obj => obj.Name,
                                                           (obj1, obj2) => new
                                                                           {
                                                                               LogEntry = obj1,
                                                                               Account = obj2
                                                                           })
                                                     .Where(obj => obj.LogEntry.IsProcessed == false
                                                                && obj.LogEntry.Type == GuildLogEntryEntity.Types.Stash
                                                                && (obj.LogEntry.ItemId == null
                                                                 || obj.LogEntry.ItemId == 0))
                                                     .Select(obj => new
                                                                    {
                                                                        obj.LogEntry.GuildId,
                                                                        obj.LogEntry.Id,
                                                                        Coins = (int)obj.LogEntry.Coins,
                                                                        obj.Account.UserId
                                                                    })
                                                     .ToListAsync()
                                                     .ConfigureAwait(false))
            {
                if (_dbFactory.GetRepository<GuildDonationRepository>()
                              .Add(new GuildDonationEntity
                                   {
                                       GuildId = logEntry.GuildId,
                                       LogEntryId = logEntry.Id,
                                       UserId = logEntry.UserId,
                                       Value = logEntry.Coins
                                   }))
                {
                    _dbFactory.GetRepository<GuildLogEntryRepository>()
                              .Refresh(obj => obj.GuildId == logEntry.GuildId
                                           && obj.Id == logEntry.Id,
                                       obj => obj.IsProcessed = true);
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}
