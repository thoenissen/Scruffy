using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Discord;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Repositories.Discord;

/// <summary>
/// Repository for accessing <see cref="DiscordServerChannelEntity"/>
/// </summary>
public class DiscordServerChannelRepository : RepositoryBase<DiscordServerChannelQueryable, DiscordServerChannelEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public DiscordServerChannelRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Bulk import of all discord channels
    /// </summary>
    /// <param name="channels">Channels</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> BulkInsert(List<(ulong ServerId, ulong ChannelId, string Name)> channels)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #DiscordServerChannels (
                                                                       [ServerId] decimal(20,0) NOT NULL,
                                                                       [ChannelId] decimal(20,0) NOT NULL,
                                                                       [Name] nvarchar(max) NULL
                                                                   );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var table = new DataTable();
                table.Columns.Add(nameof(DiscordServerChannelEntity.ServerId), typeof(ulong));
                table.Columns.Add(nameof(DiscordServerChannelEntity.ChannelId), typeof(ulong));
                table.Columns.Add(nameof(DiscordServerChannelEntity.Name), typeof(string));

                foreach (var (serverId, channelId, name) in channels)
                {
                    table.Rows.Add(serverId, channelId, name);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#DiscordServerChannels";

                    await bulk.WriteToServerAsync(table)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [DiscordServerChannels] AS [TARGET]
                                                   USING #DiscordServerChannels AS [Source]
                                                      ON [Target].[ServerId] = [Source].[ServerId]
                                                     AND [Target].[ChannelId] = [Source].[ChannelId]
                                              WHEN MATCHED THEN
                                                  UPDATE SET [Target].[Name] = [Source].[Name]
                                              WHEN NOT MATCHED THEN
                                                  INSERT ( [ServerId], [ChannelId], [Name] )
                                                  VALUES ( [Source].[ServerId], [Source].[ChannelId], [Source].[Name] )
                                              WHEN NOT MATCHED BY SOURCE THEN
                                                  DELETE;",
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