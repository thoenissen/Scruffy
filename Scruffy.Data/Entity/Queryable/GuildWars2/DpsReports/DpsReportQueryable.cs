using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.DpsReports;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.DpsReports;

/// <summary>
/// Queryable for accessing the <see cref="DpsReportEntity"/>
/// </summary>
public class DpsReportQueryable : QueryableBase<DpsReportEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DpsReportQueryable(IQueryable<DpsReportEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}