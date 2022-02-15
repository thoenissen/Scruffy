using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord;

/// <summary>
/// Queryable for accessing the <see cref="DiscordHistoricAccountRoleAssignmentEntity"/>
/// </summary>
public class DiscordHistoricAccountRoleAssignmentQueryable : QueryableBase<DiscordHistoricAccountRoleAssignmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordHistoricAccountRoleAssignmentQueryable(IQueryable<DiscordHistoricAccountRoleAssignmentEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}