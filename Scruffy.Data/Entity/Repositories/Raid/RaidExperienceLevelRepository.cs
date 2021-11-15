using System.Linq;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Raid;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Repositories.Raid;

/// <summary>
/// Repository for accessing <see cref="RaidExperienceLevelEntity"/>
/// </summary>
public class RaidExperienceLevelRepository : RepositoryBase<RaidExperienceLevelQueryable, RaidExperienceLevelEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public RaidExperienceLevelRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Refresh experience level ranking
    /// </summary>
    public void RefreshRanks()
    {
        var currentRank = GetQuery().Count();
        long? currentLevel = null;

        while (currentRank > 0)
        {
            if (Refresh(obj => obj.SuperiorExperienceLevelId == currentLevel,
                        obj =>
                        {
                            currentLevel = obj.Id;
                            obj.Rank = currentRank;
                        }))
            {
                break;
            }

            currentRank--;
        }
    }

    #endregion // Methods
}