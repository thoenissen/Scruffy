using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Queryable.Web;

/// <summary>
/// Queryable for accessing the <see cref="UserTokenEntity"/>
/// </summary>
public class UserTokenQueryable : QueryableBase<UserTokenEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public UserTokenQueryable(IQueryable<UserTokenEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}