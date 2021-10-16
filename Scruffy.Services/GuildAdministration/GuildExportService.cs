using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
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

                var itemIds = logEntries.Where(obj => obj.ItemId != null)
                                        .Select(obj => obj.ItemId)
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

                var itemIds = logEntries.Where(obj => obj.ItemId != null)
                                        .Select(obj => obj.ItemId)
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
                var logEntriesQuery = dbFactory.GetRepository<GuildLogEntryRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

                var logEntries = await dbFactory.GetRepository<GuildLogEntryRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Time > sinceDate
                                                           && obj.User != null
                                                           && obj.Type == "upgrade"
                                                           && (obj.Action == "completed"
                                                            || (obj.Action == "queued"
                                                             && logEntriesQuery.Any(obj2 => obj2.Type == "upgrade"
                                                                                         && obj2.User == obj.User
                                                                                         && obj2.Time <= obj.Time
                                                                                         && obj2.UpgradeId == obj.UpgradeId
                                                                                         && obj2.Action == "completed") == false))
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

                var itemIds = logEntries.Where(obj => obj.ItemId != null)
                                        .Select(obj => obj.ItemId)
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
                var logEntriesQuery = dbFactory.GetRepository<GuildLogEntryRepository>()
                                               .GetQuery()
                                               .Select(obj => obj);

                var logEntries = await dbFactory.GetRepository<GuildLogEntryRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Time > sinceDate
                                                           && obj.User != null
                                                           && obj.Type == "upgrade"
                                                           && (obj.Action == "completed"
                                                            || (obj.Action == "queued"
                                                             && logEntriesQuery.Any(obj2 => obj2.Type == "upgrade"
                                                                                         && obj2.User == obj.User
                                                                                         && obj2.Time <= obj.Time
                                                                                         && obj2.UpgradeId == obj.UpgradeId
                                                                                         && obj2.Action == "completed") == false))
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
                                                                   Count = obj.Sum(obj2 => obj2.Count ?? 1)
                                                               })
                                                .OrderBy(obj => obj.User)
                                                .ThenBy(obj => obj.ItemId)
                                                .ThenBy(obj => obj.UpgradeId)
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                var itemIds = logEntries.Where(obj => obj.ItemId != null)
                                        .Select(obj => obj.ItemId)
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

        /// <summary>
        /// Exporting login data
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExportLoginActivityLog(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var dateLimit = DateTime.Today.AddDays(-7);

                var entries = await dbFactory.GetRepository<AccountDailyLoginCheckRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Date >= dateLimit)
                                                .Select(obj => new
                                                               {
                                                                   obj.GuildWarsAccountEntity.Name
                                                               })
                                                .Distinct()
                                                .ToListAsync()
                                                .ConfigureAwait(false);

                await using (var memoryStream = new MemoryStream())
                {
                    await using (var writer = new StreamWriter(memoryStream))
                    {
                        await writer.WriteLineAsync("AccountName")
                                    .ConfigureAwait(false);

                        foreach (var entry in entries)
                        {
                            await writer.WriteLineAsync($"{entry.Name};")
                                        .ConfigureAwait(false);
                        }

                        await writer.FlushAsync()
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        await commandContext.Channel
                                            .SendMessageAsync(new DiscordMessageBuilder().WithFile("activity_log.csv", memoryStream))
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Exporting representation state
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExportRepresentation(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var guildId = dbFactory.GetRepository<GuildRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.DiscordServerId == commandContext.Guild.Id)
                                           .Select(obj => obj.GuildId)
                                           .FirstOrDefault();

                var entries = await dbFactory.GetRepository<AccountRepository>()
                                             .GetQuery()
                                             .Select(obj => new
                                                            {
                                                                obj.Name,
                                                                DiscordAccountId = obj.User
                                                                                      .DiscordAccounts
                                                                                      .Select(obj2 => obj2.Id)
                                                                                      .FirstOrDefault(),
                                                                obj.ApiKey
                                                            })
                                             .ToListAsync()
                                             .ConfigureAwait(false);

                var accounts = new List<(string User, string AccountName, int Characters, int?Representation)>();

                foreach (var entry in entries)
                {
                    DiscordMember user = null;

                    try
                    {
                        user = await commandContext.Guild
                                                   .GetMemberAsync(entry.DiscordAccountId)
                                                   .ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    if (user != null)
                    {
                        await using (var connector = new GuidWars2ApiConnector(entry.ApiKey))
                        {
                            var characters = await connector.GetCharactersAsync()
                                                            .ConfigureAwait(false);

                            accounts.Add((user.TryGetDisplayName(), entry.Name, characters?.Count ?? 0, characters?.Count(obj => obj.Guild == guildId) ?? 0));
                        }
                    }
                }

                await using (var memoryStream = new MemoryStream())
                {
                    await using (var writer = new StreamWriter(memoryStream))
                    {
                        await writer.WriteLineAsync("User;AccountName;Characters;Representation;Percentage")
                                    .ConfigureAwait(false);

                        foreach (var (user, accountName, characters, representation) in accounts.OrderBy(obj => obj.User)
                                                                                                                                                     .ThenBy(obj => obj.AccountName))
                        {
                            await writer.WriteLineAsync($"{user};{accountName};{characters};{representation};{(characters != 0 ? representation / (double)characters : 0)}")
                                        .ConfigureAwait(false);
                        }

                        await writer.FlushAsync()
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        await commandContext.Channel
                                            .SendMessageAsync(new DiscordMessageBuilder().WithFile("representation.csv", memoryStream))
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Exporting guild members
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExportGuildMembers(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var guild = dbFactory.GetRepository<GuildRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.DiscordServerId == commandContext.Guild.Id)
                                           .Select(obj => new
                                                          {
                                                              obj.GuildId,
                                                              obj.ApiKey
                                                          })
                                           .FirstOrDefault();

                var accounts = await dbFactory.GetRepository<AccountRepository>()
                                             .GetQuery()
                                             .Select(obj => obj.Name.ToLower())
                                             .ToListAsync()
                                             .ConfigureAwait(false);

                var members = new List<(string Name, DateTime? Joined, bool IsApiKeyValid)>();

                await using (var connector = new GuidWars2ApiConnector(guild.ApiKey))
                {
                    foreach (var member in await connector.GetGuildMembers(guild.GuildId)
                                                          .ConfigureAwait(false))
                    {
                        members.Add((member.Name, member.Joined, accounts.Contains(member.Name.ToLower())));
                    }
                }

                await using (var memoryStream = new MemoryStream())
                {
                    await using (var writer = new StreamWriter(memoryStream))
                    {
                        await writer.WriteLineAsync("AccountName;Joined;API-Key")
                                    .ConfigureAwait(false);

                        foreach (var (name, joined, isApiKeyValid) in members.OrderBy(obj => obj.IsApiKeyValid).ThenBy(obj => obj.Name))
                        {
                            await writer.WriteLineAsync($"{name};{joined?.ToString("g", LocalizationGroup.CultureInfo)};{(isApiKeyValid ? "✔️" : "❌")}")
                                        .ConfigureAwait(false);
                        }

                        await writer.FlushAsync()
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        await commandContext.Channel
                                            .SendMessageAsync(new DiscordMessageBuilder().WithFile("members.csv", memoryStream))
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion // Methods
    }
}
