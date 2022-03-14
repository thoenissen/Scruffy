using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.GameData;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsCustomRecipeEntryEntity"/>
/// </summary>
public class GuildWarsCustomRecipeEntryQueryable : QueryableBase<GuildWarsCustomRecipeEntryEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsCustomRecipeEntryQueryable(IQueryable<GuildWarsCustomRecipeEntryEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}