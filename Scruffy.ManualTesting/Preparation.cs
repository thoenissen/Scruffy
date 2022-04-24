using System;
using System.IO;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Scruffy.ManualTesting;

/// <summary>
/// Test preparation
/// </summary>
internal static class Preparation
{
    /// <summary>
    /// Discord client
    /// </summary>
    public static DiscordSocketClient DiscordClient { get; private set; }

    /// <summary>
    /// Setting up environment variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task SetEnvironmentVariables()
    {
        var configurationFile = EnvironmentSettings.UseProductiveConfiguration
                                    ? "..\\..\\..\\..\\Scruffy.ServiceHosts.Discord\\Docker_Productive.env"
                                    : "..\\..\\..\\..\\Scruffy.ServiceHosts.Discord\\Docker.env";

        foreach (var line in await File.ReadAllLinesAsync(configurationFile).ConfigureAwait(false))
        {
            var spitted = line.Split("=");

            Environment.SetEnvironmentVariable(spitted[0], spitted[1]);
        }
    }

    /// <summary>
    /// Setting up discord client
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task SetUpDiscordClient()
    {
        var config = new DiscordSocketConfig
                     {
                         LogLevel = LogSeverity.Info,
                         MessageCacheSize = 100,
                         GatewayIntents = GatewayIntents.All
                     };

        DiscordClient = new DiscordSocketClient(config);

        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"))
                            .ConfigureAwait(false);

        await DiscordClient.StartAsync()
                            .ConfigureAwait(false);
    }
}