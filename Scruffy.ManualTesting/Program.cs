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
globalCommand.DefaultMemberPermissions = GuildPermission.Administrator;
globalCommand.IsDMEnabled = false;

var globalCommand2 = new SlashCommandBuilder();
globalCommand2.WithName("account");
globalCommand2.WithDescription("Account configuration");
globalCommand2.IsDMEnabled = true;

var globalCommand3 = new SlashCommandBuilder();
globalCommand3.WithName("info");
globalCommand3.WithDescription("Information about Scruffy");
globalCommand3.IsDMEnabled = true;

// With global commands we don't need the guild.
await Preparation.DiscordClient.BulkOverwriteGlobalApplicationCommandsAsync(new[] { globalCommand.Build(), globalCommand2.Build(), globalCommand3.Build() });