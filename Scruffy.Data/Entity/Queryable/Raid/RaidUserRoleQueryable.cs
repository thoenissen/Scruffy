﻿using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidUserRoleEntity"/>
/// </summary>
public class RaidUserRoleQueryable : QueryableBase<RaidUserRoleEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidUserRoleQueryable(IQueryable<RaidUserRoleEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}