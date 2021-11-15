using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Statistics;

namespace Scruffy.Data.Entity.Queryable.Statistics;

/// <summary>
/// Queryable for accessing the <see cref="DiscordVoiceTimeSpanEntity"/>
/// </summary>
public class DiscordVoiceTimeSpanQueryable : QueryableBase<DiscordVoiceTimeSpanEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordVoiceTimeSpanQueryable(IQueryable<DiscordVoiceTimeSpanEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}