using System.IO;

using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.GameData;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Extensions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild;

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

            var now = DateTime.Now;
            var customValues = await dbFactory.GetRepository<GuildWarsItemRepository>()
                                              .GetQuery()
                                              .Where(obj => (obj.CustomValueValidDate == null
                                                          || obj.CustomValueValidDate > now)
                                                         && obj.CustomValue != null)
                                              .ToDictionaryAsync(obj => obj.ItemId,
                                                                 obj => obj.CustomValue)
                                              .ConfigureAwait(false);

            var itemIds = logEntries.Where(obj => obj.ItemId != null)
                                    .Select(obj => obj.ItemId)
                                    .Distinct()
                                    .ToList();

            var connector = new GuidWars2ApiConnector(null);
            await using (connector.ConfigureAwait(false))
            {
                var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                        .ConfigureAwait(false);

                var items = await connector.GetItems(itemIds)
                                           .ConfigureAwait(false);

                var memoryStream = new MemoryStream();
                await using (memoryStream.ConfigureAwait(false))
                {
                    var writer = new StreamWriter(memoryStream);
                    await using (writer.ConfigureAwait(false))
                    {
                        await writer.WriteLineAsync("TimeStamp;User;Operation;ItemId;ItemName;Count;TradingPostValue;VendorValue;CustomValue")
                                    .ConfigureAwait(false);

                        foreach (var entry in logEntries)
                        {
                            var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                            var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                            if (entry.ItemId == null
                             || customValues.TryGetValue(entry.ItemId.Value, out var customValue) == false)
                            {
                                customValue = null;
                            }

                            await writer.WriteLineAsync($"{entry.Time.ToString("g", LocalizationGroup.CultureInfo)};{entry.User};{entry.Operation};{entry.ItemId};{(entry.ItemId == null || entry.ItemId == 0 ? "Coins" : item?.Name)};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{(entry.ItemId == null || entry.ItemId == 0 ? entry.Coins : item?.VendorValue)};{customValue}")
                                        .ConfigureAwait(false);
                        }

                        await writer.FlushAsync()
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        await commandContext.Channel
                                            .SendFileAsync(new FileAttachment(memoryStream, "stash_log.csv"))
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

            var now = DateTime.Now;
            var customValues = await dbFactory.GetRepository<GuildWarsItemRepository>()
                                              .GetQuery()
                                              .Where(obj => (obj.CustomValueValidDate == null
                                                          || obj.CustomValueValidDate > now)
                                                         && obj.CustomValue != null)
                                              .ToDictionaryAsync(obj => obj.ItemId,
                                                                 obj => obj.CustomValue)
                                              .ConfigureAwait(false);

            var itemIds = logEntries.Where(obj => obj.ItemId != null)
                                    .Select(obj => obj.ItemId)
                                    .Distinct()
                                    .ToList();

            var connector = new GuidWars2ApiConnector(null);
            await using (connector.ConfigureAwait(false))
            {
                var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                        .ConfigureAwait(false);

                var items = await connector.GetItems(itemIds)
                                           .ConfigureAwait(false);

                var memoryStream = new MemoryStream();
                await using (memoryStream.ConfigureAwait(false))
                {
                    var writer = new StreamWriter(memoryStream);
                    await using (writer.ConfigureAwait(false))
                    {
                        await writer.WriteLineAsync("User;Operation;ItemId;ItemName;Count;TradingPostValue;VendorValue;CustomValue")
                                    .ConfigureAwait(false);

                        foreach (var entry in logEntries)
                        {
                            var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                            var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                            if (entry.ItemId == null
                             || customValues.TryGetValue(entry.ItemId.Value, out var customValue) == false)
                            {
                                customValue = null;
                            }

                            await writer.WriteLineAsync($"{entry.User};{entry.Operation};{entry.ItemId};{(entry.ItemId == null || entry.ItemId == 0 ? "Coins" : item?.Name)};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{(entry.ItemId == null || entry.ItemId == 0 ? entry.Coins : item?.VendorValue)};{customValue}")
                                        .ConfigureAwait(false);
                        }

                        await writer.FlushAsync()
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        await commandContext.Channel
                                            .SendFileAsync(new FileAttachment(memoryStream, "stash_log.csv"))
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

                                                           // Cause of some items, who don't generate 'completed' we have to check also the 'queued' entries.
                                                        || (obj.Action == "queued"
                                                         && logEntriesQuery.Any(obj2 => obj2.Type == "upgrade"
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

            var now = DateTime.Now;
            var customValues = await dbFactory.GetRepository<GuildWarsItemRepository>()
                                              .GetQuery()
                                              .Where(obj => (obj.CustomValueValidDate == null
                                                          || obj.CustomValueValidDate > now)
                                                         && obj.CustomValue != null)
                                              .ToDictionaryAsync(obj => obj.ItemId,
                                                                 obj => obj.CustomValue)
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

            var connector = new GuidWars2ApiConnector(null);
            await using (connector.ConfigureAwait(false))
            {
                var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                        .ConfigureAwait(false);

                var items = await connector.GetItems(itemIds)
                                           .ConfigureAwait(false);

                var upgrades = await connector.GetUpgrades(upgradeIds)
                                              .ConfigureAwait(false);

                var memoryStream = new MemoryStream();
                await using (memoryStream.ConfigureAwait(false))
                {
                    var writer = new StreamWriter(memoryStream);
                    await using (writer.ConfigureAwait(false))
                    {
                        await writer.WriteLineAsync("TimeStamp;User;ItemId;ItemName;Count;TradingPostValue;VendorValue;CustomValue")
                                    .ConfigureAwait(false);

                        foreach (var entry in logEntries)
                        {
                            var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                            var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                            var itemName = entry.ItemId == null ?
                                               upgrades.FirstOrDefault(obj => obj.Id == entry.UpgradeId)?.Name
                                               : item?.Name;

                            if (entry.ItemId == null
                             || customValues.TryGetValue(entry.ItemId.Value, out var customValue) == false)
                            {
                                customValue = null;
                            }

                            await writer.WriteLineAsync($"{entry.Time.ToString("g", LocalizationGroup.CultureInfo)};{entry.User};{entry.ItemId};{itemName};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{item?.VendorValue};{customValue}")
                                        .ConfigureAwait(false);
                        }

                        await writer.FlushAsync()
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        await commandContext.Channel
                                            .SendFileAsync(new FileAttachment(memoryStream, "upgrades_log.csv"))
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

                                                           // Cause of some items, who don't generate 'completed' we have to check also the 'queued' entries.
                                                        || (obj.Action == "queued"
                                                         && logEntriesQuery.Any(obj2 => obj2.Type == "upgrade"
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

            var now = DateTime.Now;
            var customValues = await dbFactory.GetRepository<GuildWarsItemRepository>()
                                              .GetQuery()
                                              .Where(obj => (obj.CustomValueValidDate == null
                                                          || obj.CustomValueValidDate > now)
                                                         && obj.CustomValue != null)
                                              .ToDictionaryAsync(obj => obj.ItemId,
                                                                 obj => obj.CustomValue)
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

            var connector = new GuidWars2ApiConnector(null);
            await using (connector.ConfigureAwait(false))
            {
                var tradingsPostValues = await connector.GetTradingPostPrices(itemIds)
                                                        .ConfigureAwait(false);

                var items = await connector.GetItems(itemIds)
                                           .ConfigureAwait(false);

                var upgrades = await connector.GetUpgrades(upgradeIds)
                                              .ConfigureAwait(false);

                var memoryStream = new MemoryStream();
                await using (memoryStream.ConfigureAwait(false))
                {
                    var writer = new StreamWriter(memoryStream);
                    await using (writer.ConfigureAwait(false))
                    {
                        await writer.WriteLineAsync("User;ItemId;ItemName;Count;TradingPostValue;VendorValue;CustomValue")
                                    .ConfigureAwait(false);

                        foreach (var entry in logEntries)
                        {
                            var item = items.FirstOrDefault(obj => obj.Id == entry.ItemId);
                            var tradingPostPrice = tradingsPostValues.FirstOrDefault(obj => obj.Id == entry.ItemId);

                            var itemName = entry.ItemId == null ?
                                               upgrades.FirstOrDefault(obj => obj.Id == entry.UpgradeId)?.Name
                                               : item?.Name;

                            if (entry.ItemId == null
                             || customValues.TryGetValue(entry.ItemId.Value, out var customValue) == false)
                            {
                                customValue = null;
                            }

                            await writer.WriteLineAsync($"{entry.User};{entry.ItemId};{itemName};{entry.Count};{tradingPostPrice?.TradingPostSellValue?.UnitPrice};{item?.VendorValue};{customValue}")
                                        .ConfigureAwait(false);
                        }

                        await writer.FlushAsync()
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        await commandContext.Channel
                                            .SendFileAsync(new FileAttachment(memoryStream, "upgrades_log.csv"))
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
                                                            obj.Account.Name
                                                        })
                                         .Distinct()
                                         .ToListAsync()
                                         .ConfigureAwait(false);

            var memoryStream = new MemoryStream();
            await using (memoryStream.ConfigureAwait(false))
            {
                var writer = new StreamWriter(memoryStream);
                await using (writer.ConfigureAwait(false))
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
                                        .SendFileAsync(new FileAttachment(memoryStream, "activity_log.csv"))
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
                IGuildUser user = null;

                try
                {
                    user = await commandContext.Guild
                                               .GetUserAsync(entry.DiscordAccountId)
                                               .ConfigureAwait(false);
                }
                catch
                {
                }

                if (user != null)
                {
                    var connector = new GuidWars2ApiConnector(entry.ApiKey);
                    await using (connector.ConfigureAwait(false))
                    {
                        var characters = await connector.GetCharactersAsync()
                                                        .ConfigureAwait(false);

                        accounts.Add((user.TryGetDisplayName(), entry.Name, characters?.Count ?? 0, characters?.Count(obj => obj.Guild == guildId) ?? 0));
                    }
                }
            }

            var memoryStream = new MemoryStream();
            await using (memoryStream.ConfigureAwait(false))
            {
                var writer = new StreamWriter(memoryStream);
                await using (writer.ConfigureAwait(false))
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
                                        .SendFileAsync(new FileAttachment(memoryStream, "representation.csv"))
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
                                 .First();

            var accounts = await dbFactory.GetRepository<AccountRepository>()
                                          .GetQuery()
                                          .Select(obj => new
                                                         {
                                                             Name = obj.Name.ToLower(),
                                                             Permission = obj.Permissions
                                                         })
                                          .ToListAsync()
                                          .ConfigureAwait(false);

            var members = new List<(string Name, DateTime? Joined, bool IsApiKeyValid, bool HasAllPermissions)>();

            var connector = new GuidWars2ApiConnector(guild.ApiKey);
            await using (connector.ConfigureAwait(false))
            {
                foreach (var member in await connector.GetGuildMembers(guild.GuildId)
                                                      .ConfigureAwait(false))
                {
                    var account = accounts.FirstOrDefault(obj => obj.Name == member.Name.ToLower());

                    members.Add((member.Name,
                                 member.Joined,
                                 account != null,
                                 account?.Permission.HasFlag(GuildWars2ApiPermission.RequiredPermissions) == true));
                }
            }

            var memoryStream = new MemoryStream();
            await using (memoryStream.ConfigureAwait(false))
            {
                var writer = new StreamWriter(memoryStream);
                await using (writer.ConfigureAwait(false))
                {
                    await writer.WriteLineAsync("AccountName;Joined;API-Key;Permissions")
                                .ConfigureAwait(false);

                    foreach (var (name, joined, isApiKeyValid, hasAllPermissions) in members.OrderBy(obj => obj.IsApiKeyValid).ThenBy(obj => obj.Name))
                    {
                        await writer.WriteLineAsync($"{name};{joined?.ToString("g", LocalizationGroup.CultureInfo)};{(isApiKeyValid ? "✔️" : "❌")};{(hasAllPermissions ? "✔️" : "❌")}")
                                    .ConfigureAwait(false);
                    }

                    await writer.FlushAsync()
                                .ConfigureAwait(false);

                    memoryStream.Position = 0;

                    await commandContext.Channel
                                        .SendFileAsync(new FileAttachment(memoryStream, "members.csv"))
                                        .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Exporting guild roles
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExportGuildRoles(CommandContextContainer commandContext)
    {
        var members = new List<(string Role, string User)>();

        foreach (var user in await commandContext.Guild
                                                 .GetUsersAsync()
                                                 .ConfigureAwait(false))
        {
            foreach (var role in user.RoleIds)
            {
                members.Add((commandContext.Guild.Roles.FirstOrDefault(obj => obj.Id == role)?.Name, user.TryGetDisplayName()));
            }
        }

        var memoryStream = new MemoryStream();

        await using (memoryStream.ConfigureAwait(false))
        {
            var writer = new StreamWriter(memoryStream);

            await using (writer.ConfigureAwait(false))
            {
                await writer.WriteLineAsync("Role;User")
                            .ConfigureAwait(false);

                foreach (var (role, user) in members.OrderBy(obj => obj.Role)
                                                    .ThenBy(obj => obj.User))
                {
                    await writer.WriteLineAsync($"{role};{user}")
                                .ConfigureAwait(false);
                }

                await writer.FlushAsync()
                            .ConfigureAwait(false);

                memoryStream.Position = 0;

                await commandContext.Channel
                                    .SendFileAsync(new FileAttachment(memoryStream, "roles.csv"))
                                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Exporting current guild rank points
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="sinceDate">Since date</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExportGuildRankPoints(CommandContextContainer commandContext, DateTime sinceDate)
    {
        var members = new Dictionary<ulong, string>();

        foreach (var user in await commandContext.Guild
                                                 .GetUsersAsync()
                                                 .ConfigureAwait(false))
        {
            members[user.Id] = user.TryGetDisplayName();
        }

        var memoryStream = new MemoryStream();

        await using (memoryStream.ConfigureAwait(false))
        {
            var writer = new StreamWriter(memoryStream);

            await using (writer.ConfigureAwait(false))
            {
                await writer.WriteLineAsync("Date;User;Login")
                            .ConfigureAwait(false);

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var discordAccounts = dbFactory.GetRepository<DiscordAccountRepository>()
                                                   .GetQuery()
                                                   .Select(obj => obj);

                    foreach (var entry in dbFactory.GetRepository<GuildRankCurrentPointsRepository>()
                                                   .GetQuery()
                                                   .Where(obj => obj.Date >= sinceDate
                                                              && obj.Guild.DiscordServerId == commandContext.Guild.Id
                                                              && obj.Type == GuildRankPointType.Login)
                                                   .Select(obj => new
                                                   {
                                                       obj.Date,
                                                       DiscordUserId = discordAccounts.Where(obj2 => obj2.UserId == obj.UserId)
                                                                                                     .Select(obj2 => (ulong?)obj2.Id)
                                                                                                     .FirstOrDefault(),
                                                       obj.Points
                                                   })
                                                   .ToList())
                    {
                        await writer.WriteLineAsync($"{entry.Date:yyyy-MM-dd};{(members.TryGetValue(entry.DiscordUserId ?? 0, out var userDisplayName) ? userDisplayName : "Unknown")};{entry.Points}")
                                    .ConfigureAwait(false);
                    }
                }

                await writer.FlushAsync()
                            .ConfigureAwait(false);

                memoryStream.Position = 0;

                await commandContext.Channel
                                    .SendFileAsync(new FileAttachment(memoryStream, "roles.csv"))
                                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Exporting current guild rank assignments
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExportGuildRankAssignments(CommandContextContainer commandContext)
    {
        var members = new Dictionary<ulong, string>();

        foreach (var user in await commandContext.Guild
                                                 .GetUsersAsync()
                                                 .ConfigureAwait(false))
        {
            members[user.Id] = user.TryGetDisplayName();
        }

        var memoryStream = new MemoryStream();

        await using (memoryStream.ConfigureAwait(false))
        {
            var writer = new StreamWriter(memoryStream);

            await using (writer.ConfigureAwait(false))
            {
                await writer.WriteLineAsync("User;Role;RoleOrder;Assignment")
                            .ConfigureAwait(false);

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var discordAccounts = dbFactory.GetRepository<DiscordAccountRepository>()
                                                   .GetQuery()
                                                   .Select(obj => obj);

                    foreach (var entry in dbFactory.GetRepository<GuildRankAssignmentRepository>()
                                                   .GetQuery()
                                                   .Where(obj => obj.Guid.DiscordServerId == commandContext.Guild.Id)
                                                   .Select(obj => new
                                                                  {
                                                                      DiscordUserId = discordAccounts.Where(obj2 => obj2.UserId == obj.UserId)
                                                                                                     .Select(obj2 => (ulong?)obj2.Id)
                                                                                                     .FirstOrDefault(),
                                                                      obj.Rank.InGameName,
                                                                      obj.Rank.Order,
                                                                      obj.TimeStamp
                                                                  })
                                                   .ToList())
                    {
                        await writer.WriteLineAsync($"{(members.TryGetValue(entry.DiscordUserId ?? 0, out var userDisplayName) ? userDisplayName : "Unknown")};{entry.InGameName};{entry.Order};{entry.TimeStamp.ToString("g", LocalizationGroup.CultureInfo)}")
                                    .ConfigureAwait(false);
                    }
                }

                await writer.FlushAsync()
                            .ConfigureAwait(false);

                memoryStream.Position = 0;

                await commandContext.Channel
                                    .SendFileAsync(new FileAttachment(memoryStream, "assignments.csv"))
                                    .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods
}