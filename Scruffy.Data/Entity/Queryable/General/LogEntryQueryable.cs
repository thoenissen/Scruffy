using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.General;

namespace Scruffy.Data.Entity.Queryable.General;

/// <summary>
/// Queryable for accessing the <see cref="LogEntryEntity"/>
/// </summary>
public class LogEntryQueryable : QueryableBase<LogEntryEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public LogEntryQueryable(IQueryable<LogEntryEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}