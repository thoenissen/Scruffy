using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.GameData;
using Scruffy.Data.Json.GuildWars2.Items;
using Scruffy.Data.Json.GuildWars2.TradingPost;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild
{
    /// <summary>
    /// Item value calculation
    /// </summary>
    public sealed class GuildWars2ItemValueCalculator : LocatedServiceBase, IDisposable
    {
        #region Fields

        /// <summary>
        /// Repository factory
        /// </summary>
        private readonly RepositoryFactory _dbFactory;

        /// <summary>
        /// Items
        /// </summary>
        private Dictionary<int, Item> _items;

        /// <summary>
        /// Trading post values
        /// </summary>
        private Dictionary<int, TradingPostItemPrice> _tradingsPostValues;

        /// <summary>
        /// Recipes
        /// </summary>
        private Dictionary<int, ItemRecipe> _recipes;

        /// <summary>
        /// Conversion
        /// </summary>
        private Dictionary<int, int?> _conversions;

        /// <summary>
        /// Custom values
        /// </summary>
        private List<CustomValueData> _customValues;

        /// <summary>
        /// Connector
        /// </summary>
        private GuildWars2ApiConnector _connector;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="dbFactory">Repository factory</param>
        public GuildWars2ItemValueCalculator(LocalizationService localizationService, RepositoryFactory dbFactory)
            : base(localizationService)
        {
            _dbFactory = dbFactory;
            _items = new Dictionary<int, Item>();
            _tradingsPostValues = new Dictionary<int, TradingPostItemPrice>();
            _recipes = new Dictionary<int, ItemRecipe>();
            _conversions = new Dictionary<int, int?>();
            _connector = new GuildWars2ApiConnector(null);
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Get item values
        /// </summary>
        /// <param name="itemId">Item Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<ItemValue>> GetItemValues(int itemId)
        {
            _customValues ??= await _dbFactory.GetRepository<GuildWarsItemRepository>()
                                              .GetQuery()
                                              .Where(obj => obj.CustomValue != null)
                                              .Select(obj => new CustomValueData
                                                             {
                                                                 ItemId = obj.ItemId,
                                                                 Value = obj.CustomValue.Value,
                                                                 IsCustomValueThresholdActivated = obj.IsCustomValueThresholdActivated
                                                             })
                                              .ToListAsync()
                                              .ConfigureAwait(false);

            var items = new List<ItemValue>();

            var startItem = await GetValue(itemId, items).ConfigureAwait(false);

            startItem.Count = 1;

            return items;
        }

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="itemValues">Item values</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<ItemValue> GetValue(int itemId, List<ItemValue> itemValues)
        {
            var itemValue = new ItemValue
                            {
                                Id = itemId
                            };

            itemValues.Add(itemValue);

            var customValue = _customValues.FirstOrDefault(obj => obj.ItemId == itemId);

            if (customValue != null)
            {
                itemValue.Value = itemId;
            }

            if (_items.TryGetValue(itemId, out var item) == false)
            {
                item = await _connector.GetItem(itemId)
                                       .ConfigureAwait(false);

                if (item == null)
                {
                    var itemEntity = _dbFactory.GetRepository<GuildWarsItemRepository>()
                                               .GetQuery()
                                               .FirstOrDefault(obj => obj.ItemId == itemId);

                    if (itemEntity != null)
                    {
                        item = new Item
                               {
                                   Id = itemId,
                                   VendorValue = itemEntity.VendorValue
                               };
                    }
                }

                _items[itemId] = item;
            }

            if (item != null)
            {
                itemValue.Name = item.Name;

                if (itemValue.Value == null)
                {
                    if (item.Flags?.Any(obj => obj is "SoulbindOnAcquire" or "AccountBound") != true)
                    {
                        if (_tradingsPostValues.TryGetValue(itemId, out var tradingPostValue) == false)
                        {
                            var prices = await _connector.GetTradingPostPrices(new List<int?> { itemId } )
                                                         .ConfigureAwait(false);

                            tradingPostValue = _tradingsPostValues[itemId] = prices.FirstOrDefault();
                        }

                        if (tradingPostValue?.TradingPostSellValue.UnitPrice > 0)
                        {
                            itemValue.Value = tradingPostValue.TradingPostSellValue.UnitPrice;
                        }
                        else if (item.VendorValue > 0)
                        {
                            itemValue.Value = item.VendorValue.Value;
                        }
                    }

                    if (itemValue.Value == null)
                    {
                        if (_recipes.TryGetValue(itemId, out var recipe) == false)
                        {
                            var ingredients = await _dbFactory.GetRepository<GuildWarsCustomRecipeEntryRepository>()
                                                              .GetQuery()
                                                              .Where(obj => obj.ItemId == itemId)
                                                              .ToListAsync()
                                                              .ConfigureAwait(false);

                            if (ingredients.Count > 0)
                            {
                                recipe = new ItemRecipe
                                         {
                                             Ingredients = ingredients.Select(obj2 => new ItemIngredient
                                                                                      {
                                                                                          ItemId = obj2.IngredientItemId,
                                                                                          Count = obj2.IngredientCount,
                                                                                      })
                                                                      .ToList(),
                                             OutputItemCount = 1
                                         };
                            }
                            else
                            {
                                recipe = await _connector.GetRecipe(itemId)
                                                         .ConfigureAwait(false);
                            }

                            _recipes[itemId] = recipe;
                        }

                        if (recipe != null)
                        {
                            if (recipe.Ingredients?.Count > 0)
                            {
                                foreach (var ingredient in recipe.Ingredients)
                                {
                                    var subItem = await GetValue(ingredient.ItemId, itemValues)
                                                      .ConfigureAwait(false);

                                    subItem.Count = ingredient.Count;
                                }
                            }

                            if (recipe.GuildIngredients?.Count > 0)
                            {
                                foreach (var ingredient in recipe.GuildIngredients)
                                {
                                    if (_conversions.TryGetValue(ingredient.UpgradeId, out var upgradeItemId) == false)
                                    {
                                        upgradeItemId = _conversions[ingredient.UpgradeId] = _dbFactory.GetRepository<GuildWarsItemGuildUpgradeConversionRepository>()
                                                                                                       .GetQuery()
                                                                                                       .Where(obj => obj.UpgradeId == ingredient.UpgradeId)
                                                                                                       .Select(obj => (int?)obj.ItemId)
                                                                                                       .FirstOrDefault();
                                    }

                                    if (upgradeItemId != null)
                                    {
                                        var subItem = await GetValue(upgradeItemId.Value, itemValues)
                                                          .ConfigureAwait(false);

                                        subItem.Count = ingredient.Count;
                                    }
                                    else
                                    {
                                        itemValues.Add(new ItemValue
                                                       {
                                                           IsUpgrade = true,
                                                           Id = ingredient.UpgradeId,
                                                           Name = "Missing upgrade conversion"
                                                       });
                                    }
                                }
                            }
                        }
                        else
                        {
                            itemValue.ErrorMessage = "Missing recipe";
                        }
                    }
                }
            }
            else
            {
                itemValue.Name = "?";
                itemValue.ErrorMessage = "Unknown item";
            }

            return itemValue;
        }

        #endregion // Methods

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _connector.Dispose();
        }

        #endregion // IDisposable
    }
}