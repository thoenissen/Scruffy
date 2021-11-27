using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.Account;

/// <summary>
/// Repository for accessing <see cref="GuildWarsAccountAchievementBitEntity"/>
/// </summary>
public class GuildWarsAccountAchievementBitRepository : RepositoryBase<GuildWarsAccountAchievementBitQueryable, GuildWarsAccountAchievementBitEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildWarsAccountAchievementBitRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}