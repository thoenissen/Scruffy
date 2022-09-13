using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.LookingForGroup;

namespace Scruffy.Data.Entity.Queryable.LookingForGroup
{
    /// <summary>
    /// Queryable for accessing the <see cref="LookingForGroupAppointmentEntity"/>
    /// </summary>
    public class LookingForGroupAppointmentQueryable : QueryableBase<LookingForGroupAppointmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public LookingForGroupAppointmentQueryable(IQueryable<LookingForGroupAppointmentEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}