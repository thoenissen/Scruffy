using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Queryable.Discord
{
    /// <summary>
    /// Queryable for accessing the <see cref="BlockedDiscordChannelEntity"/>
    /// </summary>
    public class BlockedDiscordChannelQueryable : QueryableBase<BlockedDiscordChannelEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryable"><see cref="IQueryable"/>-object</param>
        public BlockedDiscordChannelQueryable(IQueryable<BlockedDiscordChannelEntity> queryable)
            : base(queryable)
        {
        }

        #endregion // Constructor
    }
}
