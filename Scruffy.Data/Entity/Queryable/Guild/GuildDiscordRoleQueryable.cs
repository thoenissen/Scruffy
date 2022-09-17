using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildDiscordRoleEntity"/>
/// </summary>
public class GuildDiscordRoleQueryable : QueryableBase<GuildDiscordRoleEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildDiscordRoleQueryable(IQueryable<GuildDiscordRoleEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}