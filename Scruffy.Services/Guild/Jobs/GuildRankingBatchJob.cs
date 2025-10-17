using Scruffy.Services.Account.Jobs;
using Scruffy.Services.Core;
using Scruffy.Services.Developer.Jobs;
using Scruffy.Services.Discord.Jobs;
using Scruffy.Services.GuildWars2.Jobs;
using Scruffy.Services.Web;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Executing guild ranking relevant jobs
/// </summary>
public class GuildRankingBatchJob : BatchJob
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public GuildRankingBatchJob()
        : base([
                   typeof(AccountLoginCheckJob),
                   typeof(AchievementImportJob),
                   typeof(GuildRankImportJob),
                   typeof(CharactersImportJob),
                   typeof(DiscordRoleImportJob),
                   typeof(DiscordMemberImportJob),
                   typeof(GitHubImportJob),
                   typeof(GuildDonationCalculationJob),
                   typeof(GuildRankCurrentPointsJob),
                   typeof(GuildRankAssignmentJob),
                   typeof(GuildRankChangeNotificationJob),
                   typeof(GuildSpecialRankPointsJob),
                   typeof(GuildVisualizationRefreshJob),
                   typeof(GuildCheckUnknownUsersJob),
                   typeof(GuildCheckInactiveUsersJob),
                   typeof(UsersImportJob)
               ])
    {
    }

    #endregion // Constructor
}