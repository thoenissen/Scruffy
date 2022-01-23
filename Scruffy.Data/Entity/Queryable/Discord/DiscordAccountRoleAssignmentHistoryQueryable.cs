using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord;

/// <summary>
/// Queryable for accessing the <see cref="DiscordAccountRoleAssignmentHistoryEntity"/>
/// </summary>
public class DiscordAccountRoleAssignmentHistoryQueryable : QueryableBase<DiscordAccountRoleAssignmentHistoryEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public DiscordAccountRoleAssignmentHistoryQueryable(IQueryable<DiscordAccountRoleAssignmentHistoryEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}