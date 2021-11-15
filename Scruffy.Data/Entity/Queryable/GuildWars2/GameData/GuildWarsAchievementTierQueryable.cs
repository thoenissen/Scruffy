using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.GameData;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsAchievementTierEntity"/>
/// </summary>
public class GuildWarsAchievementTierQueryable : QueryableBase<GuildWarsAchievementTierEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsAchievementTierQueryable(IQueryable<GuildWarsAchievementTierEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}