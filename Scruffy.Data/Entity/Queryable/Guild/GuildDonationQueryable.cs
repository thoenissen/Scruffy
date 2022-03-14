using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Queryable.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildDonationEntity"/>
/// </summary>
public class GuildDonationQueryable : QueryableBase<GuildDonationEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildDonationQueryable(IQueryable<GuildDonationEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}