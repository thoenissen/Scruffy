using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Statistics;

namespace Scruffy.Data.Entity.Queryable.Statistics
{
    /// <summary>
    /// Queryable for accessing the <see cref="DiscordMessageEntity"/>
    /// </summary>
    public class DiscordMessageQueryable : QueryableBase<DiscordMessageEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public DiscordMessageQueryable(IQueryable<DiscordMessageEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
