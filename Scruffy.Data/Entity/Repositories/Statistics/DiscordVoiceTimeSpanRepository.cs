using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Statistics;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Statistics;

namespace Scruffy.Data.Entity.Repositories.Statistics;

/// <summary>
/// Repository for accessing <see cref="DiscordVoiceTimeSpanEntity"/>
/// </summary>
public class DiscordVoiceTimeSpanRepository : RepositoryBase<DiscordVoiceTimeSpanQueryable, DiscordVoiceTimeSpanEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public DiscordVoiceTimeSpanRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}