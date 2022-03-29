using Scruffy.Services.Account.Jobs;
using Scruffy.Services.Core;
using Scruffy.Services.Developer.Jobs;
using Scruffy.Services.Discord.Jobs;
using Scruffy.Services.GuildWars2.Jobs;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Executing guild ranking relevant jobs
/// </summary>
internal class GuildRankingBatchJob : BatchJob
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public GuildRankingBatchJob()
        : base(new List<Type>
               {
                   typeof(AccountLoginCheckJob),
                   typeof(AchievementImportJob),
                   typeof(GuildRankImportJob),
                   typeof(CharactersImportJob),
                   typeof(DiscordRoleImportJob),
                   typeof(GitHubImportJob),
                   typeof(GuildDonationCalculationJob),
                   typeof(GuildRankCurrentPointsJob),
                   typeof(GuildRankAssignmentJob),
                   typeof(GuildRankChangeNotificationJob),
                   typeof(GuildSpecialRankPointsJob)
               })
    {
    }

    #endregion // Constructor
}