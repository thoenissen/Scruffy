using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Hybrid;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.WebApp.Components.Base;
using Scruffy.WebApp.Components.Pages.Ranking.Data;

namespace Scruffy.WebApp.Components.Pages.Ranking;

/// <summary>
/// Message ranking page showing a user top list with levels
/// </summary>
[Authorize(Roles = "Member")]
public partial class MessageRankingPage : LocatedComponent
{
    #region Fields

    /// <summary>
    /// Is data being loaded?
    /// </summary>
    private bool _isLoading;

    /// <summary>
    /// Ranking entries
    /// </summary>
    private List<MessageRankingEntry> _entries;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Cache
    /// </summary>
    [Inject]
    private HybridCache Cache { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Return the CSS modifier class for the progress ring color based on the percentage
    /// </summary>
    /// <param name="percent">Progress percentage (0–100)</param>
    /// <returns>CSS class name for red, yellow, or green</returns>
    private static string GetProgressColorClass(double percent)
    {
        return percent switch
               {
                   < 33 => "level-ring-progress-low",
                   < 66 => "level-ring-progress-mid",
                   _ => "level-ring-progress-high"
               };
    }

    /// <summary>
    /// Calculate the cumulative messages required to reach a given level
    /// </summary>
    /// <param name="level">Target level</param>
    /// <returns>Total messages required</returns>
    private static long GetCumulativeMessagesForLevel(int level)
    {
        var total = 0L;

        for (var currentLevel = 1; currentLevel <= level; currentLevel++)
        {
            total += ((currentLevel - 1) / 2 * (55 + ((currentLevel - 2) * 10) + 55)) + 100;
        }

        return total / 10;
    }

    /// <summary>
    /// Determine the level for a given message count
    /// </summary>
    /// <param name="messageCount">Number of messages</param>
    /// <returns>Achieved level</returns>
    private static int CalculateLevel(int messageCount)
    {
        var level = 0;

        while (GetCumulativeMessagesForLevel(level + 1) <= messageCount)
        {
            level++;
        }

        return level;
    }

    /// <summary>
    /// Calculate the progress percentage towards the next level
    /// </summary>
    /// <param name="messageCount">Number of messages</param>
    /// <param name="currentLevel">Current level</param>
    /// <returns>Progress percentage (0–100)</returns>
    private static double CalculateLevelProgress(int messageCount, int currentLevel)
    {
        var currentThreshold = GetCumulativeMessagesForLevel(currentLevel);
        var nextThreshold = GetCumulativeMessagesForLevel(currentLevel + 1);
        var range = nextThreshold - currentThreshold;

        if (range <= 0)
        {
            return 0;
        }

        return Math.Clamp((double)(messageCount - currentThreshold) / range * 100, 0, 100);
    }

    /// <summary>
    /// Load ranking data
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task LoadDataAsync()
    {
        var entries = await Cache.GetOrCreateAsync("webapp/ranking/messages",
                                                   _ => ValueTask.FromResult(GetRankingEntries()),
                                                   new HybridCacheEntryOptions
                                                   {
                                                       Expiration = TimeSpan.FromMinutes(30)
                                                   })
                                 .ConfigureAwait(false);

        _entries = entries;
        _isLoading = false;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    /// <summary>
    /// Query and build ranking entries
    /// </summary>
    /// <returns>Sorted list of ranking entries</returns>
    private List<MessageRankingEntry> GetRankingEntries()
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var botAccountIds = repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                                 .GetQuery()
                                                 .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId
                                                                  && member.IsBot)
                                                 .Select(member => member.AccountId)
                                                 .ToHashSet();

            var ignoreChannelIds = repositoryFactory.GetRepository<DiscordIgnoreChannelRepository>()
                                                    .GetQuery()
                                                    .Where(channel => channel.DiscordServerId == WebAppConfiguration.DiscordServerId)
                                                    .Select(channel => channel.DiscordChannelId)
                                                    .ToHashSet();

            var messageCounts = repositoryFactory.GetRepository<DiscordMessageRepository>()
                                                 .GetQuery()
                                                 .Where(message => message.DiscordServerId == WebAppConfiguration.DiscordServerId
                                                                   && botAccountIds.Contains(message.DiscordAccountId) == false
                                                                   && ignoreChannelIds.Contains(message.DiscordChannelId) == false)
                                                 .GroupBy(message => message.DiscordAccountId)
                                                 .Select(group => new
                                                                  {
                                                                      AccountId = group.Key,
                                                                      Count = group.Count()
                                                                  })
                                                 .OrderByDescending(group => group.Count)
                                                 .ToList();

            var nameMap = repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                           .GetQuery()
                                           .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                           .Select(member => new
                                                             {
                                                                 member.AccountId,
                                                                 member.Name,
                                                                 member.AvatarUrl
                                                             })
                                           .ToDictionary(member => member.AccountId,
                                                         member => new
                                                                   {
                                                                       member.Name,
                                                                       member.AvatarUrl
                                                                   });

            return messageCounts.Where(messageCount => nameMap.ContainsKey(messageCount.AccountId))
                                .Select(messageCount =>
                                        {
                                            var level = CalculateLevel(messageCount.Count);
                                            var member = nameMap[messageCount.AccountId];

                                            return new MessageRankingEntry
                                                   {
                                                       Name = member.Name,
                                                       AvatarUrl = member.AvatarUrl,
                                                       MessageCount = messageCount.Count,
                                                       Level = level,
                                                       LevelProgressPercent = CalculateLevelProgress(messageCount.Count, level)
                                                   };
                                        })
                                .ToList();
        }
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_isLoading == false)
        {
            _isLoading = true;

            Task.Run(LoadDataAsync);
        }
    }

    #endregion // ComponentBase
}