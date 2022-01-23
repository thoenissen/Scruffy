using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.Guild;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Guild;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.Guild;

/// <summary>
/// Repository for accessing <see cref="GuildWarsGuildMemberEntity"/>
/// </summary>
public class GuildWarsGuildMemberRepository : RepositoryBase<GuildWarsGuildMemberQueryable, GuildWarsGuildMemberEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildWarsGuildMemberRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Bulk insert members
    /// </summary>
    /// <param name="guildId">Id of the guild</param>
    /// <param name="members">Members</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> BulkInsert(long guildId, IEnumerable<(string Name, string Rank)> members)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsGuildMembers (
                                                                       [GuildId] bigint NOT NULL,
                                                                       [Name] nvarchar(42) NOT NULL,
                                                                       [Rank] nvarchar(MAX) NULL
                                                                   );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var dataTable = new DataTable();
                dataTable.Columns.Add(nameof(GuildWarsGuildMemberEntity.GuildId), typeof(long));
                dataTable.Columns.Add(nameof(GuildWarsGuildMemberEntity.Name), typeof(string));
                dataTable.Columns.Add(nameof(GuildWarsGuildMemberEntity.Rank), typeof(string));

                foreach (var (name, rank) in members)
                {
                    // Achievement
                    dataTable.Rows.Add(guildId,
                                       name,
                                       rank);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#GuildWarsGuildMembers";
                    await bulk.WriteToServerAsync(dataTable)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsGuildMembers] AS [TARGET]
                                                       USING #GuildWarsGuildMembers AS [Source]
                                                          ON [Target].[GuildId] = [Source].[GuildId]
                                                         AND [Target].[Name] = [Source].[Name]
                                                           WHEN MATCHED THEN
                                                                UPDATE SET [Target].[Rank] = [Source].[Rank]
                                                           WHEN NOT MATCHED THEN 
                                                                INSERT ( [GuildId], [Name], [Rank] )
                                                                VALUES ( [Source].[GuildId], [Source].[Name], [Source].[Rank] )
                                                           WHEN NOT MATCHED BY SOURCE AND [Target].[GuildId] = @guildId THEN
                                                                DELETE;",
                                            connection);
                sqlCommand.Parameters.AddWithValue("@guildId", guildId);

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