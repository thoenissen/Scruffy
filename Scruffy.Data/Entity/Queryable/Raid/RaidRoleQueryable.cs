using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidRoleEntity"/>
/// </summary>
public class RaidRoleQueryable : QueryableBase<RaidRoleEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidRoleQueryable(IQueryable<RaidRoleEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}