using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.GameData;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Json.GuildWars2.Items;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Calculation of the guild donations
/// </summary>
public class GuildDonationCalculationJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Upgrades which don't generate a 'completed' entry
    /// </summary>
    private static readonly List<int> _upgradesWithoutCompleted = new()
                                                                  {
                                                                      73,
                                                                      100,
                                                                      144,
                                                                      190,
                                                                      219,
                                                                      239,
                                                                      277,
                                                                      332,
                                                                      333,
                                                                      372,
                                                                      400,
                                                                      409,
                                                                      414,
                                                                      457,
                                                                      463,
                                                                      471,
                                                                      475,
                                                                      503,
                                                                      508,
                                                                      537,
                                                                      549,
                                                                      560,
                                                                      599,
                                                                      676,
                                                                      699,
                                                                      703,
                                                                      730,
                                                                      837,
                                                                      846,
                                                                      862,
                                                                      864,
                                                                      935,
                                                                      963,
                                                                      1067,
                                                                      1121,
                                                                      1143,
                                                                      1202
                                                                  };

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _dbFactory;

    #endregion // Fields

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
        await ProcessCoinDonations().ConfigureAwait(false);
        await ProcessItemDonations().ConfigureAwait(false);
        await ProgressUpgradeDonations().ConfigureAwait(false);
    }

    #endregion // LocatedAsyncJob

    /// <summary>
    /// Stash coins
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ProcessCoinDonations()
    {
        foreach (var logEntry in await _dbFactory.GetRepository<GuildLogEntryRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.IsProcessed == false
                                                            && obj.Type == GuildLogEntryEntity.Types.Stash
                                                            && (obj.ItemId == null
                                                             || obj.ItemId == 0))
                                                 .Select(obj => new
                                                                {
                                                                    obj.GuildId,
                                                                    obj.Id,
                                                                    obj.Operation,
                                                                    Coins = obj.Coins ?? 0
                                                                })
                                                 .ToListAsync()
                                                 .ConfigureAwait(false))
        {
            if (_dbFactory.GetRepository<GuildDonationRepository>()
                          .Add(new GuildDonationEntity
                               {
                                   GuildId = logEntry.GuildId,
                                   LogEntryId = logEntry.Id,
                                   Value = logEntry.Operation == GuildLogEntryEntity.Operations.Withdraw
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
    }

    /// <summary>
    /// Stash items
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ProcessItemDonations()
    {
        var itemLogEntries = await _dbFactory.GetRepository<GuildLogEntryRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.IsProcessed == false
                                                        && obj.Type == GuildLogEntryEntity.Types.Stash
                                                        && obj.ItemId > 0)
                                             .Select(obj => new
                                                            {
                                                                obj.GuildId,
                                                                obj.Id,
                                                                obj.Operation,
                                                                ItemId = obj.ItemId.Value,
                                                                Count = obj.Count ?? 1
                                                            })
                                             .ToListAsync()
                                             .ConfigureAwait(false);

        var itemIds = itemLogEntries.Select(obj => (int?)obj.ItemId)
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

    /// <summary>
    /// Upgrades
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ProgressUpgradeDonations()
    {
        var conversionsQuery = _dbFactory.GetRepository<GuildWarsItemGuildUpgradeConversionRepository>()
                                         .GetQuery()
                                         .Select(obj => obj);

        var upgradeLogEntries = await _dbFactory.GetRepository<GuildLogEntryRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.IsProcessed == false
                                                           && obj.Type == GuildLogEntryEntity.Types.Upgrade
                                                           && obj.Action == GuildLogEntryEntity.Actions.Completed)
                                                .Select(obj => new
                                                               {
                                                                   obj.GuildId,
                                                                   obj.Id,
                                                                   ItemId = obj.ItemId
                                                                         ?? conversionsQuery.Where(obj2 => obj2.UpgradeId == obj.UpgradeId)
                                                                                            .Select(obj2 => (int?)obj2.ItemId)
                                                                                            .FirstOrDefault(),
                                                                   Count = obj.Count.Value
                                                               })
                                                .Where(obj => obj.ItemId > 0)
                                                .ToListAsync()
                                                .ConfigureAwait(false);

        var itemIds = upgradeLogEntries.Select(obj => obj.ItemId)
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

        var customRecipes = await _dbFactory.GetRepository<GuildWarsCustomRecipeEntryRepository>()
                                            .GetQuery()
                                            .Select(obj => obj)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

        var connector = new GuidWars2ApiConnector(null);

        var tradingsPostValues = (await connector.GetTradingPostPrices(itemIds).ConfigureAwait(false)).ToDictionary(obj => obj.Id, obj => obj);
        var items = (await connector.GetItems(itemIds).ConfigureAwait(false)).ToDictionary(obj => obj.Id, obj => obj);

        var recipes = customRecipes.GroupBy(obj => obj.ItemId)
                                   .ToDictionary(obj => obj.Key,
                                                 obj => new ItemRecipe
                                                        {
                                                            Ingredients = obj.Select(obj2 => new ItemIngredient
                                                                                             {
                                                                                                 ItemId = obj2.IngredientItemId,
                                                                                                 Count = obj2.IngredientCount,
                                                                                             })
                                                                             .ToList(),
                                                            OutputItemCount = 1
                                                        });

        var conversions = new Dictionary<int, int?>();
        var calculatedValues = new Dictionary<int, (long Value, bool IsDonationThresholdRelevant)?>();

        async Task<(long Value, bool IsDonationThresholdRelevant)?> GetValue(int itemId)
        {
            if (calculatedValues.TryGetValue(itemId, out var calculatedValue) == false)
            {
                var customValue = customValues.FirstOrDefault(obj => obj.ItemId == itemId);
                if (customValue != null)
                {
                    calculatedValue = calculatedValues[itemId] = (customValue.CustomValue, customValue.IsCustomValueThresholdActivated);
                }
                else
                {
                    if (items.TryGetValue(itemId, out var item) == false)
                    {
                        item = items[itemId] = await connector.GetItem(itemId)
                                                              .ConfigureAwait(false);
                    }

                    if (item != null)
                    {
                        if (item.Flags.Any(obj => obj is "SoulbindOnAcquire" or "AccountBound") == false)
                        {
                            if (tradingsPostValues.TryGetValue(itemId, out var tradingPostValue) == false)
                            {
                                var prices = await connector.GetTradingPostPrices(new List<int?> { itemId })
                                                            .ConfigureAwait(false);

                                tradingPostValue = tradingsPostValues[itemId] = prices.FirstOrDefault();
                            }

                            if (tradingPostValue?.TradingPostSellValue.UnitPrice > 0)
                            {
                                calculatedValue = (tradingPostValue.TradingPostSellValue.UnitPrice, false);
                            }
                            else if (item.VendorValue > 0)
                            {
                                calculatedValue = (item.VendorValue.Value, false);
                            }
                        }
                        else
                        {
                            if (recipes.TryGetValue(itemId, out var recipe) == false)
                            {
                                recipe = await connector.GetRecipe(itemId)
                                                        .ConfigureAwait(false);

                                recipes[itemId] = recipe;
                            }

                            if (recipe != null)
                            {
                                long? value = 0;

                                if (recipe.Ingredients?.Count > 0)
                                {
                                    foreach (var ingredient in recipe.Ingredients)
                                    {
                                        value += (await GetValue(ingredient.ItemId).ConfigureAwait(false))?.Value * ingredient.Count;
                                        if (value == null)
                                        {
                                            break;
                                        }
                                    }
                                }

                                if (recipe.GuildIngredients?.Count > 0)
                                {
                                    foreach (var ingredient in recipe.GuildIngredients)
                                    {
                                        if (conversions.TryGetValue(ingredient.UpgradeId, out var upgradeItemId) == false)
                                        {
                                            upgradeItemId = conversions[ingredient.UpgradeId] = _dbFactory.GetRepository<GuildWarsItemGuildUpgradeConversionRepository>()
                                                                                                          .GetQuery()
                                                                                                          .Where(obj => obj.UpgradeId == ingredient.UpgradeId)
                                                                                                          .Select(obj => (int?)obj.ItemId)
                                                                                                          .FirstOrDefault();
                                        }

                                        if (upgradeItemId != null)
                                        {
                                            value += (await GetValue(upgradeItemId.Value).ConfigureAwait(false))?.Value * ingredient.Count;
                                        }
                                        else
                                        {
                                            value = null;
                                            break;
                                        }
                                    }
                                }

                                if (value != null)
                                {
                                    calculatedValue = (value.Value / recipe.OutputItemCount, false);
                                }
                            }
                        }
                    }
                }
            }

            return calculatedValue;
        }

        // Completed entries
        foreach (var upgradeLogEntry in upgradeLogEntries.Where(obj => obj.ItemId != null))
        {
            var calculatedValue = await GetValue(upgradeLogEntry.ItemId ?? 0).ConfigureAwait(false);
            if (calculatedValue != null)
            {
                if (_dbFactory.GetRepository<GuildDonationRepository>()
                              .Add(new GuildDonationEntity
                                   {
                                       GuildId = upgradeLogEntry.GuildId,
                                       LogEntryId = upgradeLogEntry.Id,
                                       Value = calculatedValue.Value.Value,
                                       IsThresholdRelevant = calculatedValue.Value.IsDonationThresholdRelevant
                                   }))
                {
                    _dbFactory.GetRepository<GuildLogEntryRepository>()
                              .Refresh(obj => obj.GuildId == upgradeLogEntry.GuildId
                                           && obj.Id == upgradeLogEntry.Id,
                                       obj => obj.IsProcessed = true);
                }
            }
        }

        var logEntries = _dbFactory.GetRepository<GuildLogEntryRepository>()
                                   .GetQuery()
                                   .Select(obj => obj);

        // Queued entries
        foreach (var entry in await _dbFactory.GetRepository<GuildLogEntryRepository>()
                                              .GetQuery()
                                              .Where(obj => obj.IsProcessed == false
                                                         && obj.Type == GuildLogEntryEntity.Types.Upgrade
                                                         && obj.Action == GuildLogEntryEntity.Actions.Queued
                                                         && (obj.User == null
                                                          || logEntries.Any(obj2 => obj2.Time >= obj.Time
                                                                                 && obj2.UpgradeId == obj.UpgradeId
                                                                                 && obj2.Type == GuildLogEntryEntity.Types.Upgrade
                                                                                 && obj2.Action == GuildLogEntryEntity.Actions.Completed
                                                                                 && obj2.IsProcessed)))
                                              .ToListAsync()
                                              .ConfigureAwait(false))
        {
            _dbFactory.GetRepository<GuildLogEntryRepository>()
                      .Refresh(obj => obj.GuildId == entry.GuildId
                                   && obj.Id == entry.Id,
                               obj => obj.IsProcessed = true);
        }

        // Complete entries
        foreach (var entry in await _dbFactory.GetRepository<GuildLogEntryRepository>()
                                              .GetQuery()
                                              .Where(obj => obj.IsProcessed == false
                                                         && obj.Type == GuildLogEntryEntity.Types.Upgrade
                                                         && obj.Action == GuildLogEntryEntity.Actions.Complete)
                                              .ToListAsync()
                                              .ConfigureAwait(false))
        {
            _dbFactory.GetRepository<GuildLogEntryRepository>()
                      .Refresh(obj => obj.GuildId == entry.GuildId
                                   && obj.Id == entry.Id,
                               obj => obj.IsProcessed = true);
        }

        // Queued entries which don't generate a completed entry
        foreach (var upgradeLogEntry in await _dbFactory.GetRepository<GuildLogEntryRepository>()
                                                        .GetQuery()
                                                        .Where(obj => obj.IsProcessed == false
                                                                   && obj.Type == GuildLogEntryEntity.Types.Upgrade
                                                                   && obj.Action == GuildLogEntryEntity.Actions.Queued
                                                                   && _upgradesWithoutCompleted.Contains(obj.UpgradeId ?? 0))
                                                        .Select(obj => new
                                                                       {
                                                                           obj.GuildId,
                                                                           obj.Id,
                                                                           ItemId = conversionsQuery.Where(obj2 => obj2.UpgradeId == obj.UpgradeId)
                                                                                                    .Select(obj2 => (int?)obj2.ItemId)
                                                                                                    .FirstOrDefault(),
                                                                           Count = obj.Count ?? 1
                                                                       })
                                                        .Where(obj => obj.ItemId > 0)
                                                        .ToListAsync()
                                                        .ConfigureAwait(false))
        {
            var calculatedValue = await GetValue(upgradeLogEntry.ItemId ?? 0).ConfigureAwait(false);
            if (calculatedValue != null)
            {
                if (_dbFactory.GetRepository<GuildDonationRepository>()
                              .Add(new GuildDonationEntity
                                   {
                                       GuildId = upgradeLogEntry.GuildId,
                                       LogEntryId = upgradeLogEntry.Id,
                                       Value = calculatedValue.Value.Value,
                                       IsThresholdRelevant = calculatedValue.Value.IsDonationThresholdRelevant
                                   }))
                {
                    _dbFactory.GetRepository<GuildLogEntryRepository>()
                              .Refresh(obj => obj.GuildId == upgradeLogEntry.GuildId
                                           && obj.Id == upgradeLogEntry.Id,
                                       obj => obj.IsProcessed = true);
                }
            }
        }
    }
}