﻿using Microsoft.EntityFrameworkCore;
using Scruffy.Data.Entity.Queryable.Discord;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Discord;

namespace Scruffy.Data.Entity.Repositories.Discord;

/// <summary>
/// Repository for accessing <see cref="DiscordAccountEntity"/>
/// </summary>
public class DiscordAccountRepository : RepositoryBase<DiscordAccountQueryable, DiscordAccountEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>-object</param>
    public DiscordAccountRepository(ScruffyDbContext dbContext)
        : base(dbContext)
    {
    }

    #endregion // Constructor
}