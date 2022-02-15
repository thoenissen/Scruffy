using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Discord.Jobs
{
    /// <summary>
    /// Importing roles of each user
    /// </summary>
    public class DiscordRoleImportJob : LocatedAsyncJob
    {
        #region Fields

        /// <summary>
        /// Client
        /// </summary>
        private readonly DiscordSocketClient _client;

        /// <summary>
        /// Factory
        /// </summary>
        private readonly RepositoryFactory _dbFactory;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client</param>
        /// <param name="dbFactory">Factory</param>
        public DiscordRoleImportJob(DiscordSocketClient client, RepositoryFactory dbFactory)
        {
            _client = client;
            _dbFactory = dbFactory;
        }

        #endregion // Constructor

        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteOverrideAsync()
        {
            var entries = new List<(ulong ServerId, ulong UserId, ulong RoleId)>();

            foreach (var guild in _client.Guilds)
            {
                foreach (var user in guild.Users)
                {
                    foreach (var role in user.Roles)
                    {
                        entries.Add((guild.Id, user.Id, role.Id));
                    }
                }
            }

            if (await _dbFactory.GetRepository<DiscordHistoricAccountRoleAssignmentRepository>()
                                .BulkInsert(entries)
                                .ConfigureAwait(false) == false)
            {
                LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(DiscordRoleImportJob), "Discord role assignment import failed", null, _dbFactory.LastError);
            }
        }

        #endregion // LocatedAsyncJob
    }
}
