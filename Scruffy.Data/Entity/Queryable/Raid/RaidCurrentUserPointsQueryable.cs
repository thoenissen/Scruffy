using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidCurrentUserPointsEntity"/>
/// </summary>
public class RaidCurrentUserPointsQueryable : QueryableBase<RaidCurrentUserPointsEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidCurrentUserPointsQueryable(IQueryable<RaidCurrentUserPointsEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}