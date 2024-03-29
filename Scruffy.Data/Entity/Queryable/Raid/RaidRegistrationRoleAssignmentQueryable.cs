﻿using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidRegistrationRoleAssignmentEntity"/>
/// </summary>
public class RaidRegistrationRoleAssignmentQueryable : QueryableBase<RaidRegistrationRoleAssignmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidRegistrationRoleAssignmentQueryable(IQueryable<RaidRegistrationRoleAssignmentEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}