using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildWars2.Guild;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Guild;

namespace Scruffy.Data.Entity.Repositories.GuildWars2.Guild
{
    /// <summary>
    /// Repository for accessing <see cref="GuildWarsGuildMemberEntity"/>
    /// </summary>
    public class GuildWarsGuildMemberRepository : RepositoryBase<GuildWarsGuildMemberQueryable, GuildWarsGuildMemberEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildWarsGuildMemberRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}
