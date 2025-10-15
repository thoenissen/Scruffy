using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Developer;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Developer;

namespace Scruffy.Data.Entity.Repositories.Developer;

/// <summary>
/// Repository for accessing <see cref="GitHubCommitEntity"/>
/// </summary>
public class GitHubCommitRepository : RepositoryBase<GitHubCommitQueryable, GitHubCommitEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GitHubCommitRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Bulk insert
    /// </summary>
    /// <param name="entries">Entries</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> BulkInsert(IEnumerable<(string Sha, string Author, string Committer, DateTime TimeStamp)> entries)
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

                var sqlCommand = new SqlCommand(@"CREATE TABLE #GitHubCommits (
                                                        [Sha] nvarchar(40) NOT NULL,
                                                        [Author] nvarchar(max) NULL,
                                                        [Committer] nvarchar(max) NULL,
                                                        [TimeStamp] datetime2 NOT NULL );",
                                                connection);

                await using (sqlCommand.ConfigureAwait(false))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                var table = new DataTable();
                table.Columns.Add(nameof(GitHubCommitEntity.Sha), typeof(string));
                table.Columns.Add(nameof(GitHubCommitEntity.Author), typeof(string));
                table.Columns.Add(nameof(GitHubCommitEntity.Committer), typeof(string));
                table.Columns.Add(nameof(GitHubCommitEntity.TimeStamp), typeof(DateTime));

                foreach (var (sha, author, committer, timeStamp) in entries)
                {
                    table.Rows.Add(sha, author, committer, timeStamp);
                }

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "#GitHubCommits";

                    await bulk.WriteToServerAsync(table)
                              .ConfigureAwait(false);
                }

                sqlCommand = new SqlCommand(@"MERGE INTO [GitHubCommits] AS [TARGET]
                                                   USING #GitHubCommits AS [Source]
                                                      ON [Target].[Sha] = [Source].[Sha]
                                          WHEN MATCHED THEN
                                               UPDATE SET [Target].[Author] = [Source].[Author],
                                                          [Target].[Committer] = [Source].[Committer],
                                                          [Target].[TimeStamp] = [Source].[TimeStamp]
                                          WHEN NOT MATCHED THEN
                                               INSERT ( [Sha], [Author], [Committer], [TimeStamp] )
                                               VALUES ( [Source].[Sha], [Source].[Author], [Source].[Committer], [Source].[TimeStamp] )
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