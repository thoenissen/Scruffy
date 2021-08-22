using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Games;

namespace Scruffy.Data.Entity.Queryable.Games
{
    /// <summary>
    /// Queryable for accessing the <see cref="GameChannelEntity"/>
    /// </summary>
    public class GameChannelQueryable : QueryableBase<GameChannelEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GameChannelQueryable(IQueryable<GameChannelEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
