using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Guild member rank import
/// </summary>
public class GuildRankImportJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Guild rank service
    /// </summary>
    private readonly GuildRankService _guildRankService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guildRankService">Guild rank service</param>
    public GuildRankImportJob(GuildRankService guildRankService)
    {
        _guildRankService = guildRankService;
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        await _guildRankService.ImportGuildRanks(null)
                               .ConfigureAwait(false);

        await _guildRankService.RefreshDiscordRoles(null)
                               .ConfigureAwait(false);
    }

    #endregion // LocatedAsyncJob
}