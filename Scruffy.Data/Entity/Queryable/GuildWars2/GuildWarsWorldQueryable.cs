using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Queryable.GuildWars2;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsWorldEntity"/>
/// </summary>
public class GuildWarsWorldQueryable : QueryableBase<GuildWarsWorldEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsWorldQueryable(IQueryable<GuildWarsWorldEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}