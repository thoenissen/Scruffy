using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;

namespace Scruffy.ManualTesting;

/// <summary>
/// Test preparation
/// </summary>
internal static class Preparation
{
    private static DiscordClient _discordClient;

    /// <summary>
    /// Setting up environment variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task SetEnvironmentVariables()
    {
        var configurationFile = EnvironmentSettings.UseProductiveConfiguration
                                    ? "..\\..\\..\\..\\Scruffy.ServiceHost\\Docker_Productive.env"
                                    : "..\\..\\..\\..\\Scruffy.ServiceHost\\Docker.env";

        foreach (var line in await File.ReadAllLinesAsync(configurationFile).ConfigureAwait(false))
        {
            var spitted = line.Split("=");

            Environment.SetEnvironmentVariable(spitted[0], spitted[1]);
        }
    }

    /// <summary>
    /// Setting up the localization service
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task SetUpLocalizationService()
    {
        var stream = Assembly.Load("Scruffy.Data").GetManifestResourceStream("Scruffy.Data.Resources.Languages.de-DE.json");
        if (stream != null)
        {
            await using (stream.ConfigureAwait(false))
            {
                var localizationService = new LocalizationService();
                localizationService.Load(stream);
                GlobalServiceProvider.Current.AddSingleton(localizationService);
            }
        }
    }

    /// <summary>
    /// Setting up discord client
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task SetUpDiscordClient()
    {
        var config = new DiscordConfiguration
                     {
                         Token = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"),
                         TokenType = TokenType.Bot,
                         AutoReconnect = true,
                         Intents = DiscordIntents.All,
                         LogTimestampFormat = "yyyy-MM-dd HH:mm:ss",
                         ReconnectIndefinitely = true // TODO Connection handling
                     };

        _discordClient = new DiscordClient(config);

        GlobalServiceProvider.Current.AddSingleton(_discordClient);

        await _discordClient.ConnectAsync().ConfigureAwait(false);
    }
}