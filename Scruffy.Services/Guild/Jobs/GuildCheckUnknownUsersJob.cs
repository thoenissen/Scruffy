using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Check unknown users
/// </summary>
public class GuildCheckUnknownUsersJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

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
    /// <param name="discordClient">Discord client</param>
    public GuildCheckUnknownUsersJob(RepositoryFactory repositoryFactory,
                                     DiscordSocketClient discordClient)
    {
        _repositoryFactory = repositoryFactory;
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
        var today = DateTime.Today;

        foreach (var guild in _repositoryFactory.GetRepository<GuildChannelConfigurationRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Type == GuildChannelConfigurationType.GuildRankChanges)
                                                .Select(obj => new
                                                               {
                                                                   obj.GuildId,
                                                                   obj.DiscordChannelId
                                                               })
                                                .ToList())
        {
            var guildWarsAccounts = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                      .GetQuery()
                                                      .Select(obj => obj);

            var unknownUsers = _repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.Date == today
                                                            && obj.GuildId == guild.GuildId
                                                            && guildWarsAccounts.Any(obj2 => obj2.Name == obj.Name) == false)
                                                 .Select(obj => new
                                                                {
                                                                    obj.Name,
                                                                    obj.JoinedAt
                                                                })
                                                 .OrderBy(obj => obj.JoinedAt)
                                                 .ToList();

            if (unknownUsers.Count > 0)
            {
                var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("UnknownUsers", "Unknown users"))
                                                     .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                     .WithColor(Color.Green)
                                                     .WithTimestamp(DateTime.Now);

                var stringBuilder = new StringBuilder();

                foreach (var user in unknownUsers)
                {
                    stringBuilder.Append(Format.Bold(user.Name));
                    stringBuilder.Append(" (");
                    stringBuilder.Append(user.JoinedAt?.ToString("g", LocalizationGroup.CultureInfo));
                    stringBuilder.Append(" | ");

                    var days = (DateTime.Now - user.JoinedAt)?.TotalDays.ToString("0");

                    stringBuilder.Append(days);
                    stringBuilder.Append(" ");

                    if (days == "1")
                    {
                        stringBuilder.Append(LocalizationGroup.GetText("Day", "Day"));
                    }
                    else
                    {
                        stringBuilder.Append(LocalizationGroup.GetText("Days", "Days"));
                    }

                    stringBuilder.Append(")");
                    stringBuilder.Append(Environment.NewLine);
                }

                embedBuilder.WithDescription(stringBuilder.ToString());

                var channel = await _discordClient.GetChannelAsync(guild.DiscordChannelId)
                                                  .ConfigureAwait(false);

                if (channel is ITextChannel textChannel)
                {
                    await textChannel.SendMessageAsync(embed: embedBuilder.Build())
                                     .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}