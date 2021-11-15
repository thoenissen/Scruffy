using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid;

/// <summary>
/// Queryable for accessing the <see cref="RaidExperienceAssignmentEntity"/>
/// </summary>
public class RaidExperienceAssignmentQueryable : QueryableBase<RaidExperienceAssignmentEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public RaidExperienceAssignmentQueryable(IQueryable<RaidExperienceAssignmentEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}