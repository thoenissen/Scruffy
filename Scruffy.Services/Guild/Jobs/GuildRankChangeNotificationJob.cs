using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Guild rank change notification
/// </summary>
public class GuildRankChangeNotificationJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Repository Factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// Guild rank service
    /// </summary>
    private readonly GuildRankService _guildRankService;

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordSocketClient _discordClient;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="guildRankService">Guild rank service</param>
    /// <param name="discordClient">Discord client</param>
    public GuildRankChangeNotificationJob(RepositoryFactory repositoryFactory,
                                          GuildRankService guildRankService,
                                          DiscordSocketClient discordClient)
    {
        _repositoryFactory = repositoryFactory;
        _guildRankService = guildRankService;
        _discordClient = discordClient;
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        foreach (var guild in await _repositoryFactory.GetRepository<GuildChannelConfigurationRepository>()
                                                      .GetQuery()
                                                      .Where(obj => obj.Type == GuildChannelConfigurationType.GuildLogNotification)
                                                      .Select(obj2 => new
                                                                      {
                                                                          obj2.GuildId,
                                                                          obj2.DiscordChannelId
                                                                      })
                                                      .ToListAsync()
                                                      .ConfigureAwait(false))
        {
            var embed = await _guildRankService.CheckCurrentAssignments(guild.GuildId, false)
                                               .ConfigureAwait(false);
            if (embed != null)
            {
                var channel = await _discordClient.GetChannelAsync(guild.DiscordChannelId)
                                                  .ConfigureAwait(false);
                if (channel is ITextChannel textChannel)
                {
                    await textChannel.SendMessageAsync(embed: embed)
                                     .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}