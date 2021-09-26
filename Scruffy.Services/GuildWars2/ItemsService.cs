using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2;
using Scruffy.Services.Core;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2
{
    /// <summary>
    /// Items service
    /// </summary>
    public class ItemsService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public ItemsService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Importing all items
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> ImportItems()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                await using (var connector = new GuidWars2ApiConnector(null))
                {
                    var itemIds = await connector.GetAllItemIds()
                                                 .ConfigureAwait(false);

                    var items = await connector.GetItems(itemIds.Cast<int?>().ToList())
                                               .ConfigureAwait(false);

                    return await dbFactory.GetRepository<GuildWarsItemRepository>()
                                          .BulkInsert(items)
                                          .ConfigureAwait(false)
                        && await dbFactory.GetRepository<GuildWarsItemGuildUpgradeConversionRepository>()
                                          .BulkInsert(items.Where(obj => obj.Details?.GuildUpgradeId > 0)
                                                           .Select(obj => new KeyValuePair<int, long>(obj.Id, obj.Details.GuildUpgradeId))
                                                           .ToList())
                                          .ConfigureAwait(false);
                }
            }
        }

        #endregion // Methods
    }
}
