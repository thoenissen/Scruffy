using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.Account;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsAccountRankingGuildDataEntity"/>
/// </summary>
public class GuildWarsAccountRankingGuildDataQueryable : QueryableBase<GuildWarsAccountRankingGuildDataEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsAccountRankingGuildDataQueryable(IQueryable<GuildWarsAccountRankingGuildDataEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}