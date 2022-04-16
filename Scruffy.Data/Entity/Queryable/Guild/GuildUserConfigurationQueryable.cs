using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildUserConfigurationEntity"/>
/// </summary>
public class GuildUserConfigurationQueryable : QueryableBase<GuildUserConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildUserConfigurationQueryable(IQueryable<GuildUserConfigurationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}