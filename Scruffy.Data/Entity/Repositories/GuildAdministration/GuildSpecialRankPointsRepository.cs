using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.GuildAdministration;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Enumerations.GuildAdministration;

namespace Scruffy.Data.Entity.Repositories.GuildAdministration
{
    /// <summary>
    /// Repository for accessing <see cref="GuildSpecialRankPointsEntity"/>
    /// </summary>
    public class GuildSpecialRankPointsRepository : RepositoryBase<GuildSpecialRankPointsQueryable, GuildSpecialRankPointsEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public GuildSpecialRankPointsRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Adding points
        /// </summary>
        /// <param name="configurationId">If of the configuration</param>
        /// <param name="maximumPoints">Maximum points</param>
        /// <param name="userId">Id of the user</param>
        /// <param name="pointsList">Points list</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> AddPoints(long configurationId, double maximumPoints, long userId, ICollection<double> pointsList)
        {
            var success = false;

            var dbContext = GetDbContext();

            try
            {
                dbContext.LastError = null;

                var pointsToAdd = pointsList.Sum(obj => obj);

                var userPoints = await dbContext.Set<GuildSpecialRankPointsEntity>()
                                                .FirstOrDefaultAsync(obj => obj.ConfigurationId == configurationId
                                                                         && obj.UserId == userId)
                                                .ConfigureAwait(false);

                if (pointsToAdd > 0
                 || userPoints != null)
                {
                    foreach (var points in pointsList)
                    {
                        await dbContext.Set<GuildSpecialRankProtocolEntryEntity>()
                                       .AddAsync(new GuildSpecialRankProtocolEntryEntity
                                                 {
                                                     ConfigurationId = configurationId,
                                                     TimeStamp = DateTime.Now,
                                                     Type = points > 0 ? GuildSpecialRankProtocolEntryType.PointsAdded : GuildSpecialRankProtocolEntryType.PointsRemoved,
                                                     Amount = points,
                                                     UserId = userId
                                                 })
                                       .ConfigureAwait(false);
                    }

                    if (userPoints == null)
                    {
                        userPoints = new GuildSpecialRankPointsEntity
                                     {
                                         ConfigurationId = configurationId,
                                         UserId = userId
                                     };

                        await dbContext.Set<GuildSpecialRankPointsEntity>()
                                       .AddAsync(userPoints)
                                       .ConfigureAwait(false);
                    }

                    userPoints.Points += pointsToAdd;

                    if (userPoints.Points > maximumPoints)
                    {
                        userPoints.Points = maximumPoints;
                    }
                    else if (userPoints.Points <= 0)
                    {
                        dbContext.Set<GuildSpecialRankPointsEntity>()
                                 .Remove(userPoints);
                    }

                    await dbContext.SaveChangesAsync()
                                   .ConfigureAwait(false);
                }

                success = true;
            }
            catch (Exception ex)
            {
                dbContext.LastError = ex;
            }

            return success;
        }

        #endregion // Methods
    }
}
