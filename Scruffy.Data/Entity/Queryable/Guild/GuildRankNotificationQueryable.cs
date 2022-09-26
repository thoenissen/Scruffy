using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildRankNotificationEntity"/>
/// </summary>
public class GuildRankNotificationQueryable : QueryableBase<GuildRankNotificationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildRankNotificationQueryable(IQueryable<GuildRankNotificationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}