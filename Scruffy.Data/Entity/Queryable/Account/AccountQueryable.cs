using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;

namespace Scruffy.Data.Entity.Queryable.Account;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsAccountEntity"/>
/// </summary>
public class AccountQueryable : QueryableBase<GuildWarsAccountEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public AccountQueryable(IQueryable<GuildWarsAccountEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}