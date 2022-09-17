using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidUserSpecialRoleEntity"/>
/// </summary>
public class RaidUserSpecialRoleQueryable : QueryableBase<RaidUserSpecialRoleEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidUserSpecialRoleQueryable(IQueryable<RaidUserSpecialRoleEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}