using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.DpsReports;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.DpsReports;

/// <summary>
/// Queryable for accessing the <see cref="UserDpsReportsConfigurationEntity"/>
/// </summary>
public class UserDpsReportsConfigurationQueryable : QueryableBase<UserDpsReportsConfigurationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public UserDpsReportsConfigurationQueryable(IQueryable<UserDpsReportsConfigurationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}