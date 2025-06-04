using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.DpsReports;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.DpsReports;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.DpsReports;

/// <summary>
/// Repository for accessing <see cref="UserDpsReportsConfigurationEntity"/>
/// </summary>
public class UserDpsReportsConfigurationRepository : RepositoryBase<UserDpsReportsConfigurationQueryable, UserDpsReportsConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public UserDpsReportsConfigurationRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}