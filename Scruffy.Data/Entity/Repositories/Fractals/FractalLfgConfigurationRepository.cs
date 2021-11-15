using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Fractals;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Fractals;

namespace Scruffy.Data.Entity.Repositories.Fractals;

/// <summary>
/// Repository for accessing <see cref="FractalLfgConfigurationEntity"/>
/// </summary>
public class FractalLfgConfigurationRepository : RepositoryBase<FractalLfgConfigurationQueryable, FractalLfgConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public FractalLfgConfigurationRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}