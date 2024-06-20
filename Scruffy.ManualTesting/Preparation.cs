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
                         GatewayIntents = GatewayIntents.Guilds
                                        | GatewayIntents.GuildMembers
                                        | GatewayIntents.GuildEmojis
                                        | GatewayIntents.GuildIntegrations
                                        | GatewayIntents.GuildVoiceStates
                                        | GatewayIntents.GuildPresences
                                        | GatewayIntents.GuildMessages
                                        | GatewayIntents.GuildMessageReactions
                                        | GatewayIntents.DirectMessages
                                        | GatewayIntents.DirectMessageReactions
                                        | GatewayIntents.MessageContent
                                        | GatewayIntents.GuildScheduledEvents
                     };

        DiscordClient = new DiscordSocketClient(config);

        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"))
                            .ConfigureAwait(false);

        await DiscordClient.StartAsync()
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Installation of global commands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task InstallGlobalCommands()
    {
        var config = new SlashCommandBuilder().WithName("configuration")
                                              .WithDescription("Server configuration")
                                              .WithContextTypes(InteractionContextType.Guild, InteractionContextType.PrivateChannel)
                                              .WithDefaultMemberPermissions(GuildPermission.Administrator);

        var account = new SlashCommandBuilder().WithName("account")
                                               .WithDescription("Account configuration")
                                               .WithContextTypes(InteractionContextType.Guild, InteractionContextType.PrivateChannel, InteractionContextType.BotDm);

        var info = new SlashCommandBuilder().WithName("info")
                                            .WithDescription("Information about Scruffy")
                                            .WithContextTypes(InteractionContextType.Guild, InteractionContextType.PrivateChannel, InteractionContextType.BotDm);

        await DiscordClient.BulkOverwriteGlobalApplicationCommandsAsync([config.Build(), account.Build(), info.Build()])
                           .ConfigureAwait(false);
    }
}