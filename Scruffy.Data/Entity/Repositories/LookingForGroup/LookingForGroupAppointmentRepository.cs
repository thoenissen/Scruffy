using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.LookingForGroup;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.LookingForGroup;

namespace Scruffy.Data.Entity.Repositories.LookingForGroup
{
    /// <summary>
    /// Repository for accessing <see cref="LookingForGroupAppointmentEntity"/>
    /// </summary>
    public class LookingForGroupAppointmentRepository : RepositoryBase<LookingForGroupAppointmentQueryable, LookingForGroupAppointmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public LookingForGroupAppointmentRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}