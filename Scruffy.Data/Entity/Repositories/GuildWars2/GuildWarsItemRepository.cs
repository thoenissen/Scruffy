using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Items;

namespace Scruffy.Data.Entity.Repositories.GuildWars2
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsItemEntity"/>
    /// </summary>
    public class GuildWarsItemRepository : RepositoryBase<GuildWarsItemQueryable, GuildWarsItemEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildWarsItemRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Bulk insert items
        /// </summary>
        /// <param name="items">Items</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> BulkInsert(List<Item> items)
        {
            var success = false;

            LastError = null;

            try
            {
                await using (var connection = new SqlConnection(GetDbContext().ConnectionString))
                {
                    await connection.OpenAsync()
                                    .ConfigureAwait(false);

                    await using (var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsItems (
                                                                   [ItemId] int NOT NULL,
                                                                   [Name] nvarchar(max) NULL,
                                                                   [Type] int NOT NULL,
                                                                   [VendorValue] bigint NULL
                                                               )",
                                                                 connection))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    var dataTable = new DataTable();
                    dataTable.Columns.Add(nameof(GuildWarsItemEntity.ItemId), typeof(int));
                    dataTable.Columns.Add(nameof(GuildWarsItemEntity.Name), typeof(string));
                    dataTable.Columns.Add(nameof(GuildWarsItemEntity.Type), typeof(int));
                    dataTable.Columns.Add(nameof(GuildWarsItemEntity.VendorValue), typeof(long));

                    foreach (var entry in items)
                    {
                        GuildWars2ItemType type;

                        switch (entry.Type)
                        {
                            case "Armor":
                                {
                                    type = GuildWars2ItemType.Armor;
                                }
                                break;

                            case "Back":
                                {
                                    type = GuildWars2ItemType.Back;
                                }
                                break;

                            case "Bag":
                                {
                                    type = GuildWars2ItemType.Bag;
                                }
                                break;

                            case "Consumable":
                                {
                                    type = GuildWars2ItemType.Consumable;
                                }
                                break;

                            case "Container":
                                {
                                    type = GuildWars2ItemType.Container;
                                }
                                break;

                            case "CraftingMaterial":
                                {
                                    type = GuildWars2ItemType.CraftingMaterial;
                                }
                                break;

                            case "Gathering":
                                {
                                    type = GuildWars2ItemType.Gathering;
                                }
                                break;

                            case "Gizmo":
                                {
                                    type = GuildWars2ItemType.Gizmo;
                                }
                                break;

                            case "Key":
                                {
                                    type = GuildWars2ItemType.Key;
                                }
                                break;

                            case "MiniPet":
                                {
                                    type = GuildWars2ItemType.MiniPet;
                                }
                                break;

                            case "Tool":
                                {
                                    type = GuildWars2ItemType.Tool;
                                }
                                break;

                            case "Trait":
                                {
                                    type = GuildWars2ItemType.Trait;
                                }
                                break;

                            case "Trinket":
                                {
                                    type = GuildWars2ItemType.Trinket;
                                }
                                break;

                            case "Trophy":
                                {
                                    type = GuildWars2ItemType.Trophy;
                                }
                                break;

                            case "UpgradeComponent":
                                {
                                    type = GuildWars2ItemType.UpgradeComponent;
                                }
                                break;

                            case "Weapon":
                                {
                                    type = GuildWars2ItemType.Weapon;
                                }
                                break;

                            default:
                                {
                                    continue;
                                }
                        }

                        dataTable.Rows.Add(entry.Id, entry.Name, type, entry.VendorValue ?? (object)DBNull.Value);
                    }

                    using (var bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = "#GuildWarsItems";

                        await bulk.WriteToServerAsync(dataTable)
                                  .ConfigureAwait(false);
                    }

                    await using (var sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsItems] AS [TARGET]
                                                                               USING #GuildWarsItems AS [Source]
                                                                                  ON [Target].[ItemId] = [Source].[ItemId]
                                                                   WHEN MATCHED THEN
                                                                              UPDATE 
                                                                                SET [Target].[Name] = [Source].[Name],
                                                                                    [Target].[Type] = [Source].[Type],
                                                                                    [Target].[VendorValue] = [Source].[VendorValue]
                                                                   WHEN NOT MATCHED THEN
                                                                              INSERT ( [ItemId], [Name], [Type], [VendorValue], IsValueReducingActivated )
                                                                              VALUES ( [Source].[ItemId], [Source].[Name], [Source].[Type], [Source].[VendorValue], 0); ",
                                                                 connection))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                LastError = ex;
            }

            return success;
        }

        #endregion // Methods
    }
}
