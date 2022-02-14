using Scruffy.Services.Account.Jobs;
using Scruffy.Services.Guild.Jobs;
using Scruffy.Services.GuildWars2.Jobs;
using Scruffy.Services.Statistics.Jobs;

namespace Scruffy.Services.Core;

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
                   typeof(GuildRankCurrentPointsJob),
                   typeof(GuildSpecialRankPointsJob),
               })
    {
    }

    #endregion // Constructor
}