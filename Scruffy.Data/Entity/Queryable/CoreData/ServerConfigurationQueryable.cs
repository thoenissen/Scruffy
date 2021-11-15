using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Queryable.CoreData;

/// <summary>
/// Queryable for accessing the <see cref="ServerConfigurationEntity"/>
/// </summary>
public class ServerConfigurationQueryable : QueryableBase<ServerConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public ServerConfigurationQueryable(IQueryable<ServerConfigurationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}