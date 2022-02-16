using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildDiscordActivityPointsAssignmentEntity"/>
/// </summary>
public class GuildDiscordActivityPointsAssignmentQueryable : QueryableBase<GuildDiscordActivityPointsAssignmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildDiscordActivityPointsAssignmentQueryable(IQueryable<GuildDiscordActivityPointsAssignmentEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}