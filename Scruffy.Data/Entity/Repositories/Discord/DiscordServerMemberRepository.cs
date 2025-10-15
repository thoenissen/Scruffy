using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Discord;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Repositories.Discord;

/// <summary>
/// Repository for accessing <see cref="DiscordServerMemberEntity"/>
/// </summary>
public class DiscordServerMemberRepository : RepositoryBase<DiscordServerMemberQueryable, DiscordServerMemberEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public DiscordServerMemberRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Bulk import of all discord members
    /// </summary>
    /// <param name="members">Members</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> BulkInsert(List<(ulong ServerId, ulong AccountId, string Name)> members)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #DiscordServerMembers (
                                                                       [ServerId] decimal(20,0) NOT NULL,
                                                                       [AccountId] decimal(20,0) NOT NULL,
                                                                       [Name] nvarchar(max) NULL
                                                                   );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var table = new DataTable();
                table.Columns.Add(nameof(DiscordServerMemberEntity.ServerId), typeof(ulong));
                table.Columns.Add(nameof(DiscordServerMemberEntity.AccountId), typeof(ulong));
                table.Columns.Add(nameof(DiscordServerMemberEntity.Name), typeof(string));

                foreach (var (serverId, accountId, name) in members)
                {
                    table.Rows.Add(serverId, accountId, name);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#DiscordServerMembers";

                    await bulk.WriteToServerAsync(table)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [DiscordServerMembers] AS [TARGET]
                                                   USING #DiscordServerMembers AS [Source]
                                                      ON [Target].[ServerId] = [Source].[ServerId]
                                                     AND [Target].[AccountId] = [Source].[AccountId]
                                          WHEN NOT MATCHED
                                               AND EXISTS ( SELECT TOP 1 1 FROM [DiscordAccounts] AS [Account] WHERE [Account].[Id] = [Source].[AccountId] ) THEN
                                                  INSERT ( [ServerId], [AccountId], [Name] )
                                                  VALUES ( [Source].[ServerId], [Source].[AccountId], [Source].[Name] )
                                          WHEN NOT MATCHED BY SOURCE
                                               THEN DELETE;",
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