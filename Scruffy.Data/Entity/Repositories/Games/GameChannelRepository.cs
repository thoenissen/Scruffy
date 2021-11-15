using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Games;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Games;

namespace Scruffy.Data.Entity.Repositories.Games;

/// <summary>
/// Repository for accessing <see cref="GameChannelEntity"/>
/// </summary>
public class GameChannelRepository : RepositoryBase<GameChannelQueryable, GameChannelEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GameChannelRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}