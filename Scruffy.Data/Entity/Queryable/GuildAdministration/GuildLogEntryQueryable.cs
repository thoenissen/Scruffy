using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.GuildAdministration;

/// <summary>
/// Queryable for accessing the <see cref="GuildLogEntryEntity"/>
/// </summary>
public class GuildLogEntryQueryable : QueryableBase<GuildLogEntryEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildLogEntryQueryable(IQueryable<GuildLogEntryEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}