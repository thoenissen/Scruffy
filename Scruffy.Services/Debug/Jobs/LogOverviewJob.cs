﻿using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Debug.Jobs;

/// <summary>
/// Posting the log overview
/// </summary>
public class LogOverviewJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        var debugChannel = Environment.GetEnvironmentVariable("SCRUFFY_DEBUG_CHANNEL");

        if (string.IsNullOrWhiteSpace(debugChannel) == false)
        {
            var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

            await using (serviceProvider.ConfigureAwait(false))
            {
                var discordClient = serviceProvider.GetService<DiscordSocketClient>();
                var debugService = serviceProvider.GetService<DebugService>();

                if (await discordClient.GetChannelAsync(Convert.ToUInt64(debugChannel))
                                       .ConfigureAwait(false) is IMessageChannel messageChannel)
                {
                    await debugService.PostLogOverview(messageChannel,
                                                       DateTime.Today.AddDays(-1),
                                                       true)
                                      .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}