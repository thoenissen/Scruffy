using System.Linq;

using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildAdministration;

namespace Scruffy.Data.Entity.Queryable.GuildAdministration
{
    /// <summary>
    /// Queryable for accessing the <see cref="GuildChannelConfigurationEntity"/>
    /// </summary>
    public class GuildChannelConfigurationQueryable : QueryableBase<GuildChannelConfigurationEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public GuildChannelConfigurationQueryable(IQueryable<GuildChannelConfigurationEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
