using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Queryable.CoreData;

/// <summary>
/// Queryable for accessing the <see cref="UserEntity"/>
/// </summary>
public class UserQueryable : QueryableBase<UserEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public UserQueryable(IQueryable<UserEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}