using System.Threading.Tasks;

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

// Debug context
// var context = new DebuggingContext(scope.ServiceProvider);
//
// await context.InitializeFromChannel(1429146759606567115, 699320744781545473)
//              .ConfigureAwait(false);

// Testing
// var service = scope.ServiceProvider.GetRequiredService<LogCommandHandler>();