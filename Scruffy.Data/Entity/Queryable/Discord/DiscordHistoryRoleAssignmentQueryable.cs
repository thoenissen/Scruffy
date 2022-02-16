using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord;

/// <summary>
/// Queryable for accessing the <see cref="DiscordHistoryRoleAssignmentEntity"/>
/// </summary>
public class DiscordHistoryRoleAssignmentQueryable : QueryableBase<DiscordHistoryRoleAssignmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordHistoryRoleAssignmentQueryable(IQueryable<DiscordHistoryRoleAssignmentEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}