using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.GuildAdministration;

/// <summary>
/// Queryable for accessing the <see cref="GuildSpecialRankRoleAssignmentEntity"/>
/// </summary>
public class GuildSpecialRankRoleAssignmentQueryable : QueryableBase<GuildSpecialRankRoleAssignmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildSpecialRankRoleAssignmentQueryable(IQueryable<GuildSpecialRankRoleAssignmentEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}