using System.Threading.Tasks;

using Scruffy.ManualTesting;
using Scruffy.Services.Core;

// Settings
EnvironmentSettings.UseProductiveConfiguration = true;

// Preparations
await Preparation.SetEnvironmentVariables()
                 .ConfigureAwait(false);
await Preparation.SetUpLocalizationService()
                 .ConfigureAwait(false);
await Preparation.SetUpDiscordClient()
                 .ConfigureAwait(false);

var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
await using var unused = serviceProvider.ConfigureAwait(false);

// Testing
//var service = serviceProvider.GetService<TODO>();