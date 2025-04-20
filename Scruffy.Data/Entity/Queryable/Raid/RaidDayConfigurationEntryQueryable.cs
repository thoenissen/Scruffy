using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidDayConfigurationEntity"/>
/// </summary>
public class RaidDayConfigurationEntryQueryable : QueryableBase<RaidDayConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidDayConfigurationEntryQueryable(IQueryable<RaidDayConfigurationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}