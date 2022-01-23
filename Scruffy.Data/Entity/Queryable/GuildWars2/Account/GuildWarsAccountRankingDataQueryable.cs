using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.Account;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsAccountRankingDataEntity"/>
/// </summary>
public class GuildWarsAccountRankingDataQueryable : QueryableBase<GuildWarsAccountRankingDataEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsAccountRankingDataQueryable(IQueryable<GuildWarsAccountRankingDataEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}