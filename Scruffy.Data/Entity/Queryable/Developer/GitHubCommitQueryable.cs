using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Developer;

namespace Scruffy.Data.Entity.Queryable.Developer;

/// <summary>
/// Queryable for accessing the <see cref="GitHubCommitEntity"/>
/// </summary>
public class GitHubCommitQueryable : QueryableBase<GitHubCommitEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GitHubCommitQueryable(IQueryable<GitHubCommitEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}