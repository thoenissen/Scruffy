using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidRegistrationEntity"/>
/// </summary>
public class RaidRegistrationQueryable : QueryableBase<RaidRegistrationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidRegistrationQueryable(IQueryable<RaidRegistrationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}