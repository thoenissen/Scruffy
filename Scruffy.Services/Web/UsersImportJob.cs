using Discord.WebSocket;

using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Web
{
    /// <summary>
    /// User import
    /// </summary>
    public class UsersImportJob : LocatedAsyncJob
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private readonly DiscordSocketClient _discordSocketClient;

        /// <summary>
        /// User import service
        /// </summary>
        private readonly UsersImportService _usersImportService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="discordSocketDiscordSocketClient">Discord socket client</param>
        /// <param name="usersImportService">Users import service</param>
        public UsersImportJob(DiscordSocketClient discordSocketDiscordSocketClient,
                             UsersImportService usersImportService)
        {
            _discordSocketClient = discordSocketDiscordSocketClient;
            _usersImportService = usersImportService;
        }

        #endregion // Constructor

        #region LocatedAsyncJob

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteOverrideAsync()
        {
            var guildServer = Environment.GetEnvironmentVariable("SCRUFFY_GUILD_SERVER");
            if (ulong.TryParse(guildServer, out var guildServerId))
            {
                var guild = _discordSocketClient.GetGuild(guildServerId);
                if (guild != null)
                {
                    await _usersImportService.ImportDiscordUsers(guild)
                                             .ConfigureAwait(false);
                }
            }

            var developmentServer = Environment.GetEnvironmentVariable("SCRUFFY_DEVELOPMENT_SERVER");
            var developerRole = Environment.GetEnvironmentVariable("SCURFFY_DEVELOPER_ROLE");

            if (ulong.TryParse(developmentServer, out var developmentServerId)
             && ulong.TryParse(developerRole, out var developerRoleId))
            {
                var guild = _discordSocketClient.GetGuild(developmentServerId);
                if (guild != null)
                {
                    await _usersImportService.ImportDevelopers(guild, developerRoleId)
                                             .ConfigureAwait(false);
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}