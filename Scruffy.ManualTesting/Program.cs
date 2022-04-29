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
//var service = scope.ServiceProvider.GetService<TODO>();