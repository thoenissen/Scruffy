using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity.Queryable.GuildWars2;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Items;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.GameData;

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
            var connection = new SqlConnection(GetDbContext().ConnectionString);

            await using (connection.ConfigureAwait(false))
            {
                await connection.OpenAsync()
                                .ConfigureAwait(false);

                var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsItems (
                                                                   [ItemId] int NOT NULL,
                                                                   [Name] nvarchar(max) NULL,
                                                                   [Type] int NOT NULL,
                                                                   [VendorValue] bigint NULL
                                                               )",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
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
                    var type = GuildWars2ApiDataConverter.ToItemType(entry.Type);

                    if (type != GuildWars2ItemType.Unknown)
                    {
                        dataTable.Rows.Add(entry.Id, entry.Name, type, entry.VendorValue ?? (object)DBNull.Value);
                    }
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#GuildWarsItems";

                    await bulk.WriteToServerAsync(dataTable)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsItems] AS [TARGET]
                                                                           USING #GuildWarsItems AS [Source]
                                                                              ON [Target].[ItemId] = [Source].[ItemId]
                                                               WHEN MATCHED THEN
                                                                          UPDATE 
                                                                            SET [Target].[Name] = [Source].[Name],
                                                                                [Target].[Type] = [Source].[Type],
                                                                                [Target].[VendorValue] = [Source].[VendorValue]
                                                               WHEN NOT MATCHED THEN
                                                                          INSERT ( [ItemId], [Name], [Type], [VendorValue] )
                                                                          VALUES ( [Source].[ItemId], [Source].[Name], [Source].[Type], [Source].[VendorValue] ); ",
                                            connection);

                await using (sqlCommand.ConfigureAwait(false))
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