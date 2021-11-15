using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidRoleLineupEntryEntity"/>
/// </summary>
public class RaidRoleLineupEntryQueryable : QueryableBase<RaidRoleLineupEntryEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidRoleLineupEntryQueryable(IQueryable<RaidRoleLineupEntryEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}