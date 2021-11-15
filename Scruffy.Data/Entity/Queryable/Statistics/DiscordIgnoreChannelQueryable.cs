using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Statistics;

namespace Scruffy.Data.Entity.Queryable.Statistics;

/// <summary>
/// Queryable for accessing the <see cref="DiscordIgnoreChannelEntity"/>
/// </summary>
public class DiscordIgnoreChannelQueryable : QueryableBase<DiscordIgnoreChannelEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordIgnoreChannelQueryable(IQueryable<DiscordIgnoreChannelEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}