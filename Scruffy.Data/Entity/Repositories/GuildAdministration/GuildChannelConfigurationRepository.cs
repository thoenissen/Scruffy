using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildAdministration;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Repositories.GuildAdministration;

/// <summary>
/// Repository for accessing <see cref="GuildChannelConfigurationEntity"/>
/// </summary>
public class GuildChannelConfigurationRepository : RepositoryBase<GuildChannelConfigurationQueryable, GuildChannelConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public GuildChannelConfigurationRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}