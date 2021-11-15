using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.CoreData;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Repositories.CoreData;

/// <summary>
/// Repository for accessing <see cref="ServerConfigurationEntity"/>
/// </summary>
public class ServerConfigurationRepository : RepositoryBase<ServerConfigurationQueryable, ServerConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public ServerConfigurationRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}