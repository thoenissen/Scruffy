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
}