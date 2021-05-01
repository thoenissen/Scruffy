using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid
{
    /// <summary>
    /// Queryable for accessing the <see cref="RaidAppointmentEntity"/>
    /// </summary>
    public class RaidAppointmentQueryable : QueryableBase<RaidAppointmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RaidAppointmentQueryable(IQueryable<RaidAppointmentEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
