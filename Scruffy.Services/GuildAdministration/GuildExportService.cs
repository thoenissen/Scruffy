using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildAdministration
{
    /// <summary>
    /// Exporting guild data
    /// </summary>
    public class GuildExportService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildExportService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Exporting stash data
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="sinceDate">Since date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExportStashLog(CommandContextContainer commandContext, DateTime sinceDate)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var logEntries = await dbFactory.GetRepository<GuildLogEntryRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Time > sinceDate
                                                           && obj.Type == "stash"
                                                           && obj.Guild.DiscordServerId == commandContext.Guild.Id)
                                                .OrderBy(obj => obj.Time)
                                                .Select(obj => new
                                                               {
                                                                   obj.Id,
                                                                   obj.Time,
                                                                   obj.User,
                                                                   obj.Operation,
                                                                   obj.ItemId,
                                                                   obj.Count,
                                                                   obj.Coins
                                                               })
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                var itemIds = logEntries.Select(obj => obj.ItemId)
                                        .Distinct()
                                        .ToList();

                await using (var connector = new GuidWars2ApiConnector(null))
                {
                    var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                            .ConfigureAwait(false);

                    var items = await connector.GetItems(itemIds)
                                               .ConfigureAwait(false);

                    await using (var memoryStream = new MemoryStream())
                    {
                        await using (var writer = new StreamWriter(memoryStream))
                        {
                            await writer.WriteLineAsync("TimeStamp;User;Operation;ItemId;ItemName;Count;TradingPostValue;VendorValue")
                                        .ConfigureAwait(false);

                            foreach (var entry in logEntries)
                            {
                                var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                                var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                                await writer.WriteLineAsync($"{entry.Time.ToString("g", LocalizationGroup.CultureInfo)};{entry.User};{entry.Operation};{entry.ItemId};{(entry.ItemId == null || entry.ItemId == 0 ? "Coins" : item?.Name)};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{(entry.ItemId == null || entry.ItemId == 0 ? entry.Coins : item?.VendorValue)}")
                                            .ConfigureAwait(false);
                            }

                            await writer.FlushAsync()
                                        .ConfigureAwait(false);

                            memoryStream.Position = 0;

                            await commandContext.Channel
                                                .SendMessageAsync(new DiscordMessageBuilder().WithFile("stash_log.csv", memoryStream))
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Exporting stash data
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="sinceDate">Since date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExportStashLogSummarized(CommandContextContainer commandContext, DateTime sinceDate)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var logEntries = await dbFactory.GetRepository<GuildLogEntryRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Time > sinceDate
                                                           && obj.Type == "stash"
                                                           && obj.Guild.DiscordServerId == commandContext.Guild.Id)
                                                .GroupBy(obj => new
                                                                {
                                                                    obj.User,
                                                                    obj.ItemId,
                                                                    obj.Operation,
                                                                })
                                                .Select(obj => new
                                                               {
                                                                   obj.Key.User,
                                                                   obj.Key.ItemId,
                                                                   obj.Key.Operation,
                                                                   Count = obj.Sum(obj2 => obj2.Count),
                                                                   Coins = obj.Sum(obj2 => obj2.Coins)
                                                               })
                                                .OrderBy(obj => obj.User)
                                                .ThenBy(obj => obj.ItemId)
                                                .ThenBy(obj => obj.Operation)
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                var itemIds = logEntries.Select(obj => obj.ItemId)
                                        .Distinct()
                                        .ToList();

                await using (var connector = new GuidWars2ApiConnector(null))
                {
                    var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                            .ConfigureAwait(false);

                    var items = await connector.GetItems(itemIds)
                                               .ConfigureAwait(false);

                    await using (var memoryStream = new MemoryStream())
                    {
                        await using (var writer = new StreamWriter(memoryStream))
                        {
                            await writer.WriteLineAsync("User;Operation;ItemId;ItemName;Count;TradingPostValue;VendorValue")
                                        .ConfigureAwait(false);

                            foreach (var entry in logEntries)
                            {
                                var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                                var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                                await writer.WriteLineAsync($"{entry.User};{entry.Operation};{entry.ItemId};{(entry.ItemId == null || entry.ItemId == 0 ? "Coins" : item?.Name)};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{(entry.ItemId == null || entry.ItemId == 0 ? entry.Coins : item?.VendorValue)}")
                                            .ConfigureAwait(false);
                            }

                            await writer.FlushAsync()
                                        .ConfigureAwait(false);

                            memoryStream.Position = 0;

                            await commandContext.Channel
                                                .SendMessageAsync(new DiscordMessageBuilder().WithFile("stash_log.csv", memoryStream))
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Exporting stash data
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="sinceDate">Since date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExportUpgradesLog(CommandContextContainer commandContext, DateTime sinceDate)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var logEntries = await dbFactory.GetRepository<GuildLogEntryRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Time > sinceDate
                                                           && obj.Type == "upgrade"
                                                           && obj.Action == "completed"
                                                           && obj.Guild.DiscordServerId == commandContext.Guild.Id)
                                                .OrderBy(obj => obj.Time)
                                                .Select(obj => new
                                                {
                                                    obj.Id,
                                                    obj.Time,
                                                    obj.User,
                                                    obj.ItemId,
                                                    obj.Count,
                                                    obj.UpgradeId
                                                })
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                var itemIds = logEntries.Select(obj => obj.ItemId)
                                        .Distinct()
                                        .ToList();

                var upgradeIds = logEntries.Where(obj => obj.ItemId == null
                                                     &&  obj.UpgradeId != null)
                                           .Select(obj => obj.UpgradeId)
                                           .Distinct()
                                           .ToList();

                await using (var connector = new GuidWars2ApiConnector(null))
                {
                    var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                            .ConfigureAwait(false);

                    var items = await connector.GetItems(itemIds)
                                               .ConfigureAwait(false);

                    var upgrades = await connector.GetUpgrades(upgradeIds)
                                                  .ConfigureAwait(false);

                    await using (var memoryStream = new MemoryStream())
                    {
                        await using (var writer = new StreamWriter(memoryStream))
                        {
                            await writer.WriteLineAsync("TimeStamp;User;ItemId;ItemName;Count;TradingPostValue;VendorValue")
                                        .ConfigureAwait(false);

                            foreach (var entry in logEntries)
                            {
                                var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                                var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                                var itemName = entry.ItemId == null ?
                                                   upgrades.FirstOrDefault(obj => obj.Id == entry.UpgradeId)?.Name
                                                   : item?.Name;

                                await writer.WriteLineAsync($"{entry.Time.ToString("g", LocalizationGroup.CultureInfo)};{entry.User};{entry.ItemId};{itemName};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{item?.VendorValue}")
                                            .ConfigureAwait(false);
                            }

                            await writer.FlushAsync()
                                        .ConfigureAwait(false);

                            memoryStream.Position = 0;

                            await commandContext.Channel
                                                .SendMessageAsync(new DiscordMessageBuilder().WithFile("upgrades_log.csv", memoryStream))
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Exporting stash data
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="sinceDate">Since date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExportUpgradesLogSummarized(CommandContextContainer commandContext, DateTime sinceDate)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var logEntries = await dbFactory.GetRepository<GuildLogEntryRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Time > sinceDate
                                                           && obj.Type == "upgrade"
                                                           && obj.Action == "completed"
                                                           && obj.Guild.DiscordServerId == commandContext.Guild.Id)
                                                .GroupBy(obj => new
                                                                {
                                                                    obj.User,
                                                                    obj.ItemId,
                                                                    obj.UpgradeId
                                                                })
                                                .Select(obj => new
                                                               {
                                                                   obj.Key.User,
                                                                   obj.Key.ItemId,
                                                                   obj.Key.UpgradeId,
                                                                   Count = obj.Sum(obj2 => obj2.Count)
                                                               })
                                                .OrderBy(obj => obj.User)
                                                .ThenBy(obj => obj.ItemId)
                                                .ThenBy(obj => obj.UpgradeId)
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                var itemIds = logEntries.Select(obj => obj.ItemId)
                                        .Distinct()
                                        .ToList();

                var upgradeIds = logEntries.Where(obj => obj.ItemId == null
                                                     && obj.UpgradeId != null)
                                           .Select(obj => obj.UpgradeId)
                                           .Distinct()
                                           .ToList();

                await using (var connector = new GuidWars2ApiConnector(null))
                {
                    var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                            .ConfigureAwait(false);

                    var items = await connector.GetItems(itemIds)
                                               .ConfigureAwait(false);

                    var upgrades = await connector.GetUpgrades(upgradeIds)
                                                  .ConfigureAwait(false);

                    await using (var memoryStream = new MemoryStream())
                    {
                        await using (var writer = new StreamWriter(memoryStream))
                        {
                            await writer.WriteLineAsync("User;ItemId;ItemName;Count;TradingPostValue;VendorValue")
                                        .ConfigureAwait(false);

                            foreach (var entry in logEntries)
                            {
                                var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                                var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                                var itemName = entry.ItemId == null ?
                                                   upgrades.FirstOrDefault(obj => obj.Id == entry.UpgradeId)?.Name
                                                   : item?.Name;

                                await writer.WriteLineAsync($"{entry.User};{entry.ItemId};{itemName};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{item?.VendorValue}")
                                            .ConfigureAwait(false);
                            }

                            await writer.FlushAsync()
                                        .ConfigureAwait(false);

                            memoryStream.Position = 0;

                            await commandContext.Channel
                                                .SendMessageAsync(new DiscordMessageBuilder().WithFile("upgrades_log.csv", memoryStream))
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        #endregion // Methods
    }
}
