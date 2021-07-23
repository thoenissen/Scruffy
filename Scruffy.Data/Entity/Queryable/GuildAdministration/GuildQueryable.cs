using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildAdministration;

namespace Scruffy.Data.Entity.Queryable.GuildAdministration
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildEntity"/>
    /// </summary>
    public class GuildQueryable : QueryableBase<GuildEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildQueryable(IQueryable<GuildEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
