using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord;

/// <summary>
/// Queryable for accessing the <see cref="DiscordServerChannelEntity"/>
/// </summary>
public class DiscordServerChannelQueryable : QueryableBase<DiscordServerChannelEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordServerChannelQueryable(IQueryable<DiscordServerChannelEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}