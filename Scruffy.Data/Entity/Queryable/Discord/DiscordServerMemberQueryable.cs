using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord;

/// <summary>
/// Queryable for accessing the <see cref="DiscordServerMemberEntity"/>
/// </summary>
public class DiscordServerMemberQueryable : QueryableBase<DiscordServerMemberEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordServerMemberQueryable(IQueryable<DiscordServerMemberEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}