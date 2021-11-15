using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord;

/// <summary>
/// Queryable for accessing the <see cref="DiscordAccountEntity"/>
/// </summary>
public class DiscordAccountQueryable : QueryableBase<DiscordAccountEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordAccountQueryable(IQueryable<DiscordAccountEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}