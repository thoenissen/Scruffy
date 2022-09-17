using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Web;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Developer;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Repositories.Web;

/// <summary>
/// Repository for accessing <see cref="UserLoginEntity"/>
/// </summary>
public class UserLoginRepository : RepositoryBase<UserLoginQueryable, UserLoginEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public UserLoginRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Bulk insert users
    /// </summary>
    /// <param name="users">Users</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> BulkInsert(List<(long UserId, ulong DiscordAccountId, string Name)> users)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #UserLogins (
	                                                            [LoginProvider] [nvarchar](450) NOT NULL,
	                                                            [ProviderKey] [nvarchar](450) NOT NULL,
	                                                            [ProviderDisplayName] [nvarchar](max) NULL,
	                                                            [UserId] [bigint] NOT NULL);",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var table = new DataTable();
                table.Columns.Add(nameof(UserLoginEntity.LoginProvider), typeof(string));
                table.Columns.Add(nameof(UserLoginEntity.ProviderKey), typeof(string));
                table.Columns.Add(nameof(UserLoginEntity.ProviderDisplayName), typeof(string));
                table.Columns.Add(nameof(UserLoginEntity.UserId), typeof(long));

                foreach (var (userId, discordAccountId, name) in users)
                {
                    table.Rows.Add("Discord", discordAccountId.ToString(), name, userId);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#UserLogins";

                    await bulk.WriteToServerAsync(table)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [UserLogins] AS [TARGET]
                                                       USING #UserLogins AS [Source]
                                                          ON [Target].[LoginProvider] = [Source].[LoginProvider]
                                                         AND [Target].[ProviderKey] = [Source].[ProviderKey]
                                                  WHEN NOT MATCHED THEN
                                                       INSERT ( [LoginProvider], [ProviderKey], [ProviderDisplayName], [UserId] )
                                                       VALUES ( [Source].[LoginProvider], [Source].[ProviderKey], [Source].[ProviderDisplayName], [Source].[UserId] )
                                                  WHEN NOT MATCHED BY SOURCE THEN DELETE;",
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