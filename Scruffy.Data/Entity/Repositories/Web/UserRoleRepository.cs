using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Web;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Repositories.Web;

/// <summary>
/// Repository for accessing <see cref="UserRoleEntity"/>
/// </summary>
public class UserRoleRepository : RepositoryBase<UserRoleQueryable, UserRoleEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public UserRoleRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Bulk insert developers
    /// </summary>
    /// <param name="developers">Developers</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> BulkInsertDevelopers(List<long> developers) => BulkInsertRoles(developers, 1);

    /// <summary>
    /// Bulk insert administrators
    /// </summary>
    /// <param name="administrators">Administrators</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> BulkInsertAdministrators(List<long> administrators) => BulkInsertRoles(administrators, 2);

    /// <summary>
    /// Bulk insert members
    /// </summary>
    /// <param name="members">Members</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> BulkInsertMembers(List<long> members) => BulkInsertRoles(members, 3);

    /// <summary>
    /// Bulk insert privileged members
    /// </summary>
    /// <param name="privilegedMembers">Privileged members</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> BulkInsertPrivilegedMembers(List<long> privilegedMembers) => BulkInsertRoles(privilegedMembers, 4);

    /// <summary>
    /// Bulk insert role assignments
    /// </summary>
    /// <param name="users">Users</param>
    /// <param name="roleId">Role id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<bool> BulkInsertRoles(List<long> users, long roleId)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #UserRoles (
	                                                            	[UserId] [bigint] NOT NULL,
                                                                    [RoleId] [bigint] NOT NULL);",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var table = new DataTable();
                table.Columns.Add(nameof(UserRoleEntity.UserId), typeof(long));
                table.Columns.Add(nameof(UserRoleEntity.RoleId), typeof(long));

                foreach (var userId in users)
                {
                    table.Rows.Add(userId, roleId);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#UserRoles";

                    await bulk.WriteToServerAsync(table)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [UserRoles] AS [TARGET]
                                                       USING #UserRoles AS [Source]
                                                          ON [Target].[UserId] = [Source].[UserId]
                                                         AND [Target].[RoleId] = [Source].[RoleId]
                                                  WHEN NOT MATCHED THEN
                                                       INSERT ( [UserId], [RoleId] )
                                                       VALUES ( [Source].[UserId], [Source].[RoleId] )
                                                  WHEN NOT MATCHED BY SOURCE AND [Target].[RoleId] = @roleId THEN DELETE;",
                                            connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.Parameters.AddWithValue("@roleId", roleId);
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