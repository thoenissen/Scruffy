using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Fractals;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Fractals;

namespace Scruffy.Data.Entity.Repositories.Fractals
{
    /// <summary>
    /// Repository for accessing <see cref="FractalAppointmentEntity"/>
    /// </summary>
    public class FractalAppointmentRepository : RepositoryBase<FractalAppointmentQueryable, FractalAppointmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public FractalAppointmentRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}
