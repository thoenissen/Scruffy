using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Committing a raid appointment
    /// </summary>
    public class RaidCommitService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidCommitService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Commit a raid appointment
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="aliasName">Alias name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommitRaidAppointment(CommandContext commandContext, string aliasName)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var now = DateTime.Now;

                var appointmentId = await dbFactory.GetRepository<RaidAppointmentRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.TimeStamp < now
                                                                                  && obj.IsCommitted == false
                                                                                  && obj.RaidDayConfiguration.AliasName == aliasName)
                                                 .Select(obj => obj.Id)
                                                 .FirstOrDefaultAsync()
                                                 .ConfigureAwait(false);

                if (appointmentId > 0)
                {
                }
                else
                {
                    await commandContext.RespondAsync(LocalizationGroup.GetText("NoOpenAppointment", "There is no uncommitted appointment available."))
                                        .ConfigureAwait(false);
                }
            }
        }

        #endregion // Methods

    }
}
