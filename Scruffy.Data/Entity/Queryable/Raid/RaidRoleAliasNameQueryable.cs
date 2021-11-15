using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidRoleAliasNameEntity"/>
/// </summary>
public class RaidRoleAliasNameQueryable : QueryableBase<RaidRoleAliasNameEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidRoleAliasNameQueryable(IQueryable<RaidRoleAliasNameEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}