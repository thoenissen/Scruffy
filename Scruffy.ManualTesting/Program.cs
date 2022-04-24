using System.Threading.Tasks;

using Discord;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.ManualTesting;
using Scruffy.Services.Core;

// Settings
EnvironmentSettings.UseProductiveConfiguration = false;

// Preparations
await Preparation.SetEnvironmentVariables()
                 .ConfigureAwait(false);
await Preparation.SetUpDiscordClient()
                 .ConfigureAwait(false);

var serviceProvider = new ServiceProviderContainer();
await using var unused = serviceProvider.ConfigureAwait(false);

serviceProvider.Initialize(obj => obj.AddSingleton(Preparation.DiscordClient));

using var scope = serviceProvider.CreateScope();

// Testing
//var service = scope.GetService<TODO>();

var globalCommand = new SlashCommandBuilder();
globalCommand.WithName("configuration");
globalCommand.WithDescription("Server configuration");

// With global commands we don't need the guild.
await Preparation.DiscordClient.BulkOverwriteGlobalApplicationCommandsAsync(new[] { globalCommand.Build() });