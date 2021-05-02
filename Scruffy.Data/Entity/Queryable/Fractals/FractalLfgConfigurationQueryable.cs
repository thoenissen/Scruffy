using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Fractals;

namespace Scruffy.Data.Entity.Queryable.Fractals
{
    /// <summary>
    /// Queryable for accessing the <see cref="FractalLfgConfigurationEntity"/>
    /// </summary>
    public class FractalLfgConfigurationQueryable : QueryableBase<FractalLfgConfigurationEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public FractalLfgConfigurationQueryable(IQueryable<FractalLfgConfigurationEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
