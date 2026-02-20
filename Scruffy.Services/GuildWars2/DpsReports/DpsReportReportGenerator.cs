using System.Threading;

using GW2EIJSON;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.DpsReports;
using Scruffy.Data.Json.DpsReport;
using Scruffy.Data.Services.DpsReport;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Generator for DPS report statistics
/// </summary>
public class DpsReportReportGenerator
{
    #region Constants

    /// <summary>
    /// ID for the alacrity buff
    /// </summary>
    private const int AlacrityId = 30328;

    /// <summary>
    /// ID for the quickness buff
    /// </summary>
    private const int QuicknessId = 1187;

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repositoryFactory">Repository factory</param>
    public DpsReportReportGenerator(RepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region Public methods

    /// <summary>
    /// Gets overall statistics for a report
    /// </summary>
    /// <param name="detailedReport">Detailed report</param>
    /// <returns>Overall statistics</returns>
    public static OverallStatistics GetOverallStatistics(JsonLog detailedReport)
    {
        return new OverallStatistics
               {
                   Dps = detailedReport.Players?.Sum(player => player.DpsTargets?.Sum(dpsTarget => dpsTarget.Count > 0 ? dpsTarget[0].Dps : 0)),
                   Alacrity = GetUptime(detailedReport.Players, AlacrityId),
                   Quickness = GetUptime(detailedReport.Players, QuicknessId),
               };
    }

    /// <summary>
    /// Gets personal statistics for a report
    /// </summary>
    /// <param name="detailedReport">Detailed report</param>
    /// <param name="accountNames">Guild Wars 2 account names of the user</param>
    /// <returns>Personal statistics</returns>
    public static PersonalStatistics GetPersonalStatistics(JsonLog detailedReport, IEnumerable<string> accountNames)
    {
        var playerCharacterName = GetOwnCharacterName(detailedReport, accountNames);
        var statistics = new PersonalStatistics
                         {
                             PlayerCharacterName = playerCharacterName,
                             Mechanics = GetMechanics(detailedReport, playerCharacterName),
                         };

        var player = detailedReport?.Players?.FirstOrDefault(player => player.Name?.Equals(playerCharacterName, StringComparison.OrdinalIgnoreCase) == true);

        if (player != null)
        {
            var alacrityBuff = player.GroupBuffs?.FirstOrDefault(buff => buff.Id == AlacrityId)?.BuffData;

            if (alacrityBuff?.Count > 0
                && alacrityBuff[0].Generation > 15.0D)
            {
                statistics.Alacrity = alacrityBuff[0].Generation;

                statistics.PlayerRole = player.Healing > 0
                                            ? "Alacrity Healer"
                                            : "Alacrity DPS";
            }
            else
            {
                var quicknessBuff = player.GroupBuffs?.FirstOrDefault(buff => buff.Id == QuicknessId)?.BuffData;

                if (quicknessBuff?.Count > 0
                    && quicknessBuff[0].Generation > 15.0D)
                {
                    statistics.Quickness = quicknessBuff[0].Generation;

                    statistics.PlayerRole = player.Healing > 0
                                                ? "Quickness Healer"
                                                : "Quickness DPS";
                }
            }

            if (player.Healing == 0)
            {
                statistics.PlayerDps = player.DpsTargets?.Sum(dpsTarget => dpsTarget.Count > 0 ? dpsTarget[0].Dps : 0);
            }
        }

        statistics.PlayerRole ??= "DPS";

        return statistics;
    }

    /// <summary>
    /// Gets the encounters for a user in a specific week
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="weekStart">Start of the week</param>
    /// <param name="weekEnd">End of the week</param>
    /// <returns>List of encounters with success information</returns>
    public List<DpsReportExpansionEntry> GetEncounters(int userId, CancellationToken token, DateTime weekStart, DateTime weekEnd)
    {
        var encounters = DpsReportAnalyzer.GetEncounters();

        token.ThrowIfCancellationRequested();

        var dpsReportRepository = _repositoryFactory.GetRepository<DpsReportRepository>();

        var bosses = dpsReportRepository.GetQuery()
                                        .Where(r => r.UserId == userId
                                                    && r.EncounterTime >= weekStart
                                                    && r.EncounterTime < weekEnd)
                                        .GroupBy(r => r.BossId)
                                        .ToDictionary(g => g.Key,
                                                      g => g.Any(r => r.IsSuccess));

        token.ThrowIfCancellationRequested();

        foreach (var boss in encounters.SelectMany(expansion => expansion.Encounters.SelectMany(encounter => encounter.Bosses)))
        {
            boss.IsSuccessful = null;

            foreach (var bossId in boss.BossIds)
            {
                if (bosses.TryGetValue((long)bossId, out var isSuccessful))
                {
                    boss.IsSuccessful = boss.IsSuccessful == true || isSuccessful;
                }
            }
        }

        return encounters;
    }

    #endregion // Public methods

    #region Private methods

    /// <summary>
    /// Gets the own character name from the players list
    /// </summary>
    /// <param name="report">Detailed report</param>
    /// <param name="accountNames">Guild Wars 2 account names of the user</param>
    /// <returns>Character name of the player</returns>
    private static string GetOwnCharacterName(JsonLog report, IEnumerable<string> accountNames)
    {
        if (report?.Players == null || report.Players.Count == 0)
        {
            return null;
        }

        var player = report.Players.FirstOrDefault(player => accountNames?.Any(accountName => player.Account?.Equals(accountName, StringComparison.OrdinalIgnoreCase) == true) == true);

        return player?.Name;
    }

    /// <summary>
    /// Gets the mechanics counts for a specific player
    /// </summary>
    /// <param name="report">Detailed report</param>
    /// <param name="playerCharacterName">Character name of the player</param>
    /// <returns>List of mechanics with hit counts</returns>
    private static List<Mechanic> GetMechanics(JsonLog report, string playerCharacterName)
    {
        var result = new List<Mechanic>();

        if (report?.Mechanics == null || playerCharacterName == null)
        {
            return result;
        }

        foreach (var mechanic in report.Mechanics)
        {
            if (mechanic.MechanicsData != null)
            {
                var count = mechanic.MechanicsData.Count(m => m.Actor?.Equals(playerCharacterName, StringComparison.OrdinalIgnoreCase) == true);

                if (count > 0)
                {
                    result.Add(new Mechanic
                               {
                                   Name = mechanic.Name,
                                   FullName = mechanic.FullName,
                                   Description = mechanic.Description,
                                   Count = count
                               });
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Calculates the uptime of a specific buff across all players in the report
    /// </summary>
    /// <param name="players">Players</param>
    /// <param name="buffId">Buff ID</param>
    /// <returns>Average Uptime</returns>
    private static double? GetUptime(IReadOnlyList<JsonPlayer> players, int buffId)
    {
        if (players == null
            || players.Count == 0)
        {
            return null;
        }

        double weightedUptime = 0;
        long summedActiveTime = 0;

        foreach (var player in players)
        {
            if (player.ActiveTimes == null
                || player.ActiveTimes.Count == 0)
            {
                continue;
            }

            var playerActiveTime = player.ActiveTimes[0];
            summedActiveTime += playerActiveTime;

            if (player.BuffUptimesActive == null
                || player.BuffUptimesActive.Count == 0)
            {
                continue;
            }

            var buffUptime = player.BuffUptimesActive.FirstOrDefault(buf => buf.Id == buffId);

            if (buffUptime?.BuffData == null
                || buffUptime.BuffData.Count == 0)
            {
                continue;
            }

            weightedUptime += buffUptime.BuffData[0].Uptime * playerActiveTime;
        }

        return summedActiveTime > 0
                   ? weightedUptime / summedActiveTime
                   : null;
    }

    #endregion // Private methods
}