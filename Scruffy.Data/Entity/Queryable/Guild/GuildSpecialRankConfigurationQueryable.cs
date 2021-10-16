using System.Linq;
using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildSpecialRankConfigurationEntity"/>
    /// </summary>
    public class GuildSpecialRankConfigurationQueryable : QueryableBase<GuildSpecialRankConfigurationEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildSpecialRankConfigurationQueryable(IQueryable<GuildSpecialRankConfigurationEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
