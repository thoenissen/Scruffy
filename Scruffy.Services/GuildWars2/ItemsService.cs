using System.IO;

using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.GameData;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.Guild.DialogElements.Forms;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

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
            var connector = new GuidWars2ApiConnector(null);
            await using (connector.ConfigureAwait(false))
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

    /// <summary>
    /// Item configuration
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ConfigureItem(IContextContainer context)
    {
        var dialogHandler = new DialogHandler(context);
        await using (dialogHandler.ConfigureAwait(false))
        {
            var data = await dialogHandler.RunForm<ItemConfigurationFormData>()
                                      .ConfigureAwait(false);
            if (data != null)
            {
                var connector = new GuidWars2ApiConnector(null);
                await using (connector.ConfigureAwait(false))
                {
                    var item = await connector.GetItem(data.ItemId)
                                              .ConfigureAwait(false);
                    if (item != null)
                    {
                        using (var dbFactory = RepositoryFactory.CreateInstance())
                        {
                            if (dbFactory.GetRepository<GuildWarsItemRepository>()
                                         .AddOrRefresh(obj => obj.ItemId == data.ItemId,
                                                       obj =>
                                                       {
                                                           if (obj.ItemId == data.ItemId)
                                                           {
                                                               obj.ItemId = data.ItemId;
                                                               obj.Name = item.Name;
                                                               obj.Type = GuildWars2ApiDataConverter.ToItemType(item.Type);
                                                               obj.VendorValue = item.VendorValue;
                                                           }

                                                           obj.CustomValue = data.CustomValue;
                                                           obj.CustomValueValidDate = null;
                                                           obj.IsCustomValueThresholdActivated = data.IsThresholdItem;
                                                       }))
                            {
                                await context.SendMessageAsync(LocalizationGroup.GetText("ValueAssigned", "The value has been assigned."))
                                             .ConfigureAwait(false);
                            }
                            else
                            {
                                throw dbFactory.LastError;
                            }
                        }
                    }
                    else
                    {
                        await context.ReplyAsync(LocalizationGroup.GetText("InvalidItem", "There is no item with the specified ID."))
                                     .ConfigureAwait(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Custom value export
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExportCustomValues(IContextContainer context)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var items = await dbFactory.GetRepository<GuildWarsItemRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.CustomValue != null)
                                            .Select(obj => new
                                            {
                                                obj.ItemId,
                                                obj.Name,
                                                obj.CustomValue,
                                                obj.CustomValueValidDate,
                                                obj.IsCustomValueThresholdActivated
                                            })
                                            .OrderBy(obj => obj.ItemId)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

            var memoryStream = new MemoryStream();

            await using (memoryStream.ConfigureAwait(false))
            {
                var writer = new StreamWriter(memoryStream);

                await using (writer.ConfigureAwait(false))
                {
                    await writer.WriteLineAsync("ItemId;Name;CustomValue;CustomValueValidDate;CustomValueThreshold;")
                                .ConfigureAwait(false);

                    foreach (var entry in items)
                    {
                        await writer.WriteLineAsync($"{entry.ItemId};{entry.Name};{entry.CustomValue};{entry.CustomValueValidDate};{(entry.IsCustomValueThresholdActivated ? "✔️" : "❌")}")
                                    .ConfigureAwait(false);
                    }

                    await writer.FlushAsync()
                                .ConfigureAwait(false);

                    memoryStream.Position = 0;

                    await context.Channel
                                 .SendFileAsync(new FileAttachment(memoryStream, "custom_values.csv"))
                                 .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // Methods
}