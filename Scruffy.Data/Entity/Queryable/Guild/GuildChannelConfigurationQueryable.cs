using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildChannelConfigurationEntity"/>
/// </summary>
public class GuildChannelConfigurationQueryable : QueryableBase<GuildChannelConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildChannelConfigurationQueryable(IQueryable<GuildChannelConfigurationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}