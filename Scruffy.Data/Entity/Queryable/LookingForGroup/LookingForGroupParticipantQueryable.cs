using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.LookingForGroup;

namespace Scruffy.Data.Entity.Queryable.LookingForGroup;

/// <summary>
/// Queryable for accessing the <see cref="LookingForGroupParticipantEntity"/>
/// </summary>
public class LookingForGroupParticipantQueryable : QueryableBase<LookingForGroupParticipantEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public LookingForGroupParticipantQueryable(IQueryable<LookingForGroupParticipantEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}