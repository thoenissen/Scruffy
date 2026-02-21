using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord;

/// <summary>
/// Queryable for accessing the <see cref="DiscordMessageExperienceLogEntryEntity"/>
/// </summary>
public class DiscordMessageExperienceLogEntryQueryable : QueryableBase<DiscordMessageExperienceLogEntryEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordMessageExperienceLogEntryQueryable(IQueryable<DiscordMessageExperienceLogEntryEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}