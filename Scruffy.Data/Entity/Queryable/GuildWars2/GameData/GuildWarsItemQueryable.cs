using System.Linq;
using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.GameData
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildWarsItemEntity"/>
    /// </summary>
    public class GuildWarsItemQueryable : QueryableBase<GuildWarsItemEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildWarsItemQueryable(IQueryable<GuildWarsItemEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
