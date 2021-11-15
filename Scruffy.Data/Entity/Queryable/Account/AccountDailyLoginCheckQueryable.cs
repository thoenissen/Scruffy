using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Queryable.Account;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsAccountDailyLoginCheckEntity"/>
/// </summary>
public class AccountDailyLoginCheckQueryable : QueryableBase<GuildWarsAccountDailyLoginCheckEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public AccountDailyLoginCheckQueryable(IQueryable<GuildWarsAccountDailyLoginCheckEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}