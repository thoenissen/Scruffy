using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.ManualTesting;
using Scruffy.Services.Core;

// Settings
EnvironmentSettings.UseProductiveConfiguration = true;

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