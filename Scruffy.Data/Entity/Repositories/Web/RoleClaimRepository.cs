﻿using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Web;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Web;

namespace Scruffy.Data.Entity.Repositories.Web;

/// <summary>
/// Repository for accessing <see cref="RoleClaimEntity"/>
/// </summary>
public class RoleClaimRepository : RepositoryBase<RoleClaimQueryable, RoleClaimEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public RoleClaimRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}