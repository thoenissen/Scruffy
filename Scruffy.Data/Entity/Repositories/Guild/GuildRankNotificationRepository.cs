using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Guild;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Repositories.Guild;

/// <summary>
/// Repository for accessing <see cref="GuildRankNotificationEntity"/>
/// </summary>
public class GuildRankNotificationRepository : RepositoryBase<GuildRankNotificationQueryable, GuildRankNotificationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildRankNotificationRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}