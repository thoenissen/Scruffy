using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.Account;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsAccountDailyLoginCheckEntity"/>
/// </summary>
public class GuildWarsAccountDailyLoginCheckQueryable : QueryableBase<GuildWarsAccountDailyLoginCheckEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsAccountDailyLoginCheckQueryable(IQueryable<GuildWarsAccountDailyLoginCheckEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}