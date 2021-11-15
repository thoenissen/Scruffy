using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Raid;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Repositories.Raid;

/// <summary>
/// Repository for accessing <see cref="RaidRoleLineupHeaderEntity"/>
/// </summary>
public class RaidRoleLineupHeaderRepository : RepositoryBase<RaidRoleLineupHeaderQueryable, RaidRoleLineupHeaderEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public RaidRoleLineupHeaderRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}