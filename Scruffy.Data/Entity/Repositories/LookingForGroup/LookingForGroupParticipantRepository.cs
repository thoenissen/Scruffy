using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.LookingForGroup;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.LookingForGroup;

namespace Scruffy.Data.Entity.Repositories.LookingForGroup;

/// <summary>
/// Repository for accessing <see cref="LookingForGroupParticipantEntity"/>
/// </summary>
public class LookingForGroupParticipantRepository : RepositoryBase<LookingForGroupParticipantQueryable, LookingForGroupParticipantEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public LookingForGroupParticipantRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}