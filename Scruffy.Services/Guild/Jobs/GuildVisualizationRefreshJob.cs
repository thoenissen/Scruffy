﻿using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Refresh visualizations
/// </summary>
public class GuildVisualizationRefreshJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// Visualizations service
    /// </summary>
    private readonly GuildRankVisualizationService _visualizationService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="visualizationService">Visualization service</param>
    public GuildVisualizationRefreshJob(RepositoryFactory repositoryFactory, GuildRankVisualizationService visualizationService)
    {
        _repositoryFactory = repositoryFactory;
        _visualizationService = visualizationService;
    }

    #endregion // Constructor

    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        foreach (var entry in _repositoryFactory.GetRepository<GuildChannelConfigurationRepository>()
                                                .GetQuery()
                                                .Where(obj => obj.Type == GuildChannelConfigurationType.GuildOverviewRanking)
                                                .Select(obj => new
                                                               {
                                                                   obj.Guild.DiscordServerId,
                                                                   obj.DiscordChannelId,
                                                                   obj.DiscordMessageId
                                                               })
                                                .ToList())
        {
            if (entry.DiscordMessageId != null)
            {
                await _visualizationService.RefreshOverview(entry.DiscordServerId, entry.DiscordChannelId, entry.DiscordMessageId.Value, 0, null, true)
                                           .ConfigureAwait(false);
            }
        }
    }

    #endregion // LocatedAsyncJob
}