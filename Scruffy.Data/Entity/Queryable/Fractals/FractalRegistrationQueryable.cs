using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Fractals;

namespace Scruffy.Data.Entity.Queryable.Fractals;

/// <summary>
/// Queryable for accessing the <see cref="FractalRegistrationEntity"/>
/// </summary>
public class FractalRegistrationQueryable : QueryableBase<FractalRegistrationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public FractalRegistrationQueryable(IQueryable<FractalRegistrationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}