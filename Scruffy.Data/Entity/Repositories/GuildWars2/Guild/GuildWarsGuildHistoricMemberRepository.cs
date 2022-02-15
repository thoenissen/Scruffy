using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.Guild;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Guild;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.Guild;

/// <summary>
/// Repository for accessing <see cref="GuildWarsGuildHistoricMemberEntity"/>
/// </summary>
public class GuildWarsGuildHistoricMemberRepository : RepositoryBase<GuildWarsGuildHistoricMemberQueryable, GuildWarsGuildHistoricMemberEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildWarsGuildHistoricMemberRepository(ScruffyDbContext dbContext)
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
    public async Task<bool> BulkInsert(long guildId, IEnumerable<(string Name, string Rank, DateTime? Joined)> members)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #GuildWarsGuildHistoricMembers (
                                                                       [Date] datetime2(7) NOT NULL,
                                                                       [GuildId] bigint NOT NULL,
                                                                       [Name] nvarchar(42) NOT NULL,
                                                                       [Rank] nvarchar(MAX) NULL,
                                                                       [JoinedAt] datetime2(7) NULL
                                                                   );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var dataTable = new DataTable();
                dataTable.Columns.Add(nameof(GuildWarsGuildHistoricMemberEntity.Date), typeof(DateTime));
                dataTable.Columns.Add(nameof(GuildWarsGuildHistoricMemberEntity.GuildId), typeof(long));
                dataTable.Columns.Add(nameof(GuildWarsGuildHistoricMemberEntity.Name), typeof(string));
                dataTable.Columns.Add(nameof(GuildWarsGuildHistoricMemberEntity.Rank), typeof(string));
                dataTable.Columns.Add(nameof(GuildWarsGuildHistoricMemberEntity.JoinedAt), typeof(DateTime));

                var today = DateTime.Today;

                foreach (var (name, rank, joined) in members)
                {
                    // Achievement
                    dataTable.Rows.Add(today,
                                       guildId,
                                       name,
                                       rank,
                                       joined != null
                                           ? joined.Value
                                           : DBNull.Value);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#GuildWarsGuildHistoricMembers";
                    await bulk.WriteToServerAsync(dataTable)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [GuildWarsGuildHistoricMembers] AS [TARGET]
                                                   USING #GuildWarsGuildHistoricMembers AS [Source]
                                                      ON [Target].[Date] = [Source].[Date]
                                                     AND [Target].[GuildId] = [Source].[GuildId]
                                                     AND [Target].[Name] = [Source].[Name]
                                                       WHEN MATCHED THEN
                                                            UPDATE SET [Target].[Rank] = [Source].[Rank],
                                                                       [Target].[JoinedAt] = [Source].[JoinedAt],
                                                       WHEN NOT MATCHED THEN 
                                                            INSERT ( [GuildId], [Name], [Rank], [JoinedAt] )
                                                            VALUES ( [Source].[GuildId], [Source].[Name], [Source].[Rank], [Source].[JoinedAt] )
                                                       WHEN NOT MATCHED BY SOURCE 
                                                        AND [Target].[GuildId] = @guildId
                                                        AND [Target].[Date] = @date THEN
                                                            DELETE;",
                                            connection);
                sqlCommand.Parameters.AddWithValue("@guildId", guildId);
                sqlCommand.Parameters.AddWithValue("@date", today);

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