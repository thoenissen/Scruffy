using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs
{
    /// <summary>
    /// Check inactive users
    /// </summary>
    public class GuildCheckInactiveUsersJob : LocatedAsyncJob
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
        public GuildCheckInactiveUsersJob(RepositoryFactory repositoryFactory,
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
            var inactivityLimit = DateTime.Today.AddMonths(-1);

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
                var ranks = _repositoryFactory.GetRepository<GuildRankRepository>()
                                              .GetQuery()
                                              .Select(obj => obj);

                var loginQuery = _repositoryFactory.GetRepository<GuildWarsAccountDailyLoginCheckRepository>()
                                                   .GetQuery()
                                                   .Select(obj => obj);

                var accounts = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                 .GetQuery()
                                                 .Select(obj => obj);

                var members = _repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                                .GetQuery()
                                                .Select(obj => obj);

                var inactiveUsers = _repositoryFactory.GetRepository<GuildRankAssignmentRepository>()
                                                      .GetQuery()
                                                      .Where(obj => obj.GuildId == guild.GuildId
                                                                 && obj.RankId == ranks.OrderByDescending(obj2 => obj2.Order)
                                                                                       .Select(obj2 => obj2.Id)
                                                                                       .FirstOrDefault()
                                                                 && loginQuery.Any(obj2 => obj2.Account.UserId == obj.UserId
                                                                                        && obj2.Date > inactivityLimit) == false)
                                                      .Join(accounts,
                                                            obj => obj.UserId,
                                                            obj => obj.UserId,
                                                            (obj1, obj2) => new
                                                            {
                                                                obj2.Name,
                                                                obj1.TimeStamp
                                                            })
                                                      .Where(obj => members.Any(obj2 => obj2.GuildId == guild.GuildId
                                                                                       && obj2.Date == today
                                                                                       && obj2.Name == obj.Name))
                                                      .ToList();

                if (inactiveUsers.Count > 0)
                {
                    var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("InactiveUsers", "Inactive users"))
                                                         .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                         .WithColor(Color.Green)
                                                         .WithTimestamp(DateTime.Now);

                    var stringBuilder = new StringBuilder();

                    foreach (var user in inactiveUsers)
                    {
                        stringBuilder.Append(Format.Bold(user.Name));
                        stringBuilder.Append(" (");
                        stringBuilder.Append(user.TimeStamp.ToString("g", LocalizationGroup.CultureInfo));
                        stringBuilder.Append(" | ");

                        var days = (DateTime.Now - user.TimeStamp).TotalDays.ToString("0");

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
}