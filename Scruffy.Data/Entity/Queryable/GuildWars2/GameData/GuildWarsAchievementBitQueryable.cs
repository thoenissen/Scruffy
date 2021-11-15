using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.GameData;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsAchievementBitEntity"/>
/// </summary>
public class GuildWarsAchievementBitQueryable : QueryableBase<GuildWarsAchievementBitEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsAchievementBitQueryable(IQueryable<GuildWarsAchievementBitEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}