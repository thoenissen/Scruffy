using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.GameData;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.WebApi;

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

            // Stash coins
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
                                                                        obj.LogEntry.Operation,
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
                                       Value =  logEntry.Operation == GuildLogEntryEntity.Operations.Withdraw
                                                    ? -1 * logEntry.Coins
                                                    : logEntry.Coins
                                   }))
                {
                    _dbFactory.GetRepository<GuildLogEntryRepository>()
                              .Refresh(obj => obj.GuildId == logEntry.GuildId
                                           && obj.Id == logEntry.Id,
                                       obj => obj.IsProcessed = true);
                }
            }

            // Stash items
            var itemLogEntries = await _dbFactory.GetRepository<GuildLogEntryRepository>()
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
                                                            && obj.LogEntry.ItemId > 0)
                                                 .Select(obj => new
                                                                {
                                                                    obj.LogEntry.GuildId,
                                                                    obj.LogEntry.Id,
                                                                    obj.LogEntry.Operation,
                                                                    ItemId = obj.LogEntry.ItemId.Value,
                                                                    Count = obj.LogEntry.Count.Value,
                                                                    obj.Account.UserId,
                                                                })
                                                 .ToListAsync()
                                                 .ConfigureAwait(false);

            var itemIds = itemLogEntries.Select(obj => (int?)obj.Id)
                                        .Distinct()
                                        .ToList();

            var customValues = await _dbFactory.GetRepository<GuildWarsItemRepository>()
                                               .GetQuery()
                                               .Where(obj => obj.CustomValue != null)
                                               .Select(obj => new
                                                              {
                                                                  obj.ItemId,
                                                                  CustomValue = obj.CustomValue.Value,
                                                                  obj.IsCustomValueThresholdActivated
                                                              })
                                               .ToListAsync()
                                               .ConfigureAwait(false);

            var connector = new GuidWars2ApiConnector(null);

            var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                    .ConfigureAwait(false);

            var items = await connector.GetItems(itemIds)
                                       .ConfigureAwait(false);

            foreach (var itemLogEntry in itemLogEntries)
            {
                long? value = null;
                var isDonationThresholdRelevant = false;

                var customValue = customValues.FirstOrDefault(obj => obj.ItemId == itemLogEntry.ItemId);
                if (customValue != null)
                {
                    value = customValue.CustomValue;
                    isDonationThresholdRelevant = customValue.IsCustomValueThresholdActivated;
                }
                else
                {
                    value = (long?)tradingsPostValues.FirstOrDefault(obj => obj.Id == itemLogEntry.ItemId)?.TradingPostSellValue?.UnitPrice
                         ?? items.FirstOrDefault(obj => obj.Id == itemLogEntry.ItemId)?.VendorValue;
                }

                if (value != null)
                {
                    if (_dbFactory.GetRepository<GuildDonationRepository>()
                                  .Add(new GuildDonationEntity
                                       {
                                           GuildId = itemLogEntry.GuildId,
                                           LogEntryId = itemLogEntry.Id,
                                           UserId = itemLogEntry.UserId,
                                           Value = itemLogEntry.Operation == GuildLogEntryEntity.Operations.Withdraw
                                                       ? -1 * itemLogEntry.Count * value.Value
                                                       : itemLogEntry.Count * value.Value,
                                           IsThresholdRelevant = isDonationThresholdRelevant
                                       }))
                    {
                        _dbFactory.GetRepository<GuildLogEntryRepository>()
                                  .Refresh(obj => obj.GuildId == itemLogEntry.GuildId
                                               && obj.Id == itemLogEntry.Id,
                                           obj => obj.IsProcessed = true);
                    }
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}
