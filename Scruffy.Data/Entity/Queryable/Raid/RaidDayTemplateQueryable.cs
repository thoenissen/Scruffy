using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Queryable.Raid
{
    /// <summary>
    /// Queryable for accessing the <see cref="RaidDayTemplateEntity"/>
    /// </summary>
    public class RaidDayTemplateQueryable : QueryableBase<RaidDayTemplateEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public RaidDayTemplateQueryable(IQueryable<RaidDayTemplateEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
