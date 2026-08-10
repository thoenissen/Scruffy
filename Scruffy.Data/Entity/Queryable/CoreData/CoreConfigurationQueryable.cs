using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Queryable.CoreData;

/// <summary>
/// Queryable for accessing the <see cref="CoreConfigurationEntity"/>
/// </summary>
public class CoreConfigurationQueryable : QueryableBase<CoreConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public CoreConfigurationQueryable(IQueryable<CoreConfigurationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}