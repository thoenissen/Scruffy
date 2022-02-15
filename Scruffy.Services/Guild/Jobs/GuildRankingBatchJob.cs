using Scruffy.Services.Account.Jobs;
using Scruffy.Services.Core;
using Scruffy.Services.Discord.Jobs;
using Scruffy.Services.GuildWars2.Jobs;
using Scruffy.Services.Statistics.Jobs;

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
                   typeof(MessageImportJob),
                   typeof(GuildRankImportJob),
                   typeof(CharactersImportJob),
                   typeof(DiscordRoleImportJob),
                   typeof(GuildRankCurrentPointsJob),
                   typeof(GuildSpecialRankPointsJob),
               })
    {
    }

    #endregion // Constructor
}