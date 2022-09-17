using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Queryable.Web;

/// <summary>
/// Queryable for accessing the <see cref="RoleClaimEntity"/>
/// </summary>
public class RoleClaimQueryable : QueryableBase<RoleClaimEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RoleClaimQueryable(IQueryable<RoleClaimEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}