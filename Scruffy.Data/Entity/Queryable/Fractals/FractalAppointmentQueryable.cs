using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Fractals;

namespace Scruffy.Data.Entity.Queryable.Fractals
{
    /// <summary>
    /// Queryable for accessing the <see cref="FractalAppointmentEntity"/>
    /// </summary>
    public class FractalAppointmentQueryable : QueryableBase<FractalAppointmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public FractalAppointmentQueryable(IQueryable<FractalAppointmentEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
