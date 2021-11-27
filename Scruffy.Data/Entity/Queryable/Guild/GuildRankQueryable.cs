using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildRankEntity"/>
/// </summary>
public class GuildRankQueryable : QueryableBase<GuildRankEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildRankQueryable(IQueryable<GuildRankEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}