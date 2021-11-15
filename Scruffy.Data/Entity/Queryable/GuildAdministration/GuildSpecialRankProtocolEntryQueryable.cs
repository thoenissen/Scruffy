using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.GuildAdministration;

/// <summary>
/// Queryable for accessing the <see cref="GuildSpecialRankProtocolEntryEntity"/>
/// </summary>
public class GuildSpecialRankProtocolEntryQueryable : QueryableBase<GuildSpecialRankProtocolEntryEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildSpecialRankProtocolEntryQueryable(IQueryable<GuildSpecialRankProtocolEntryEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}