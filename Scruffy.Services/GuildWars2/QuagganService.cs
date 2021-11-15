using System;
using System.Threading.Tasks;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Quaggan service
/// </summary>
public class QuagganService : LocatedServiceBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public QuagganService(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Posting a random Quaggan
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostRandomQuaggan(CommandContextContainer commandContext)
    {
        var connector = new GuidWars2ApiConnector(null);
        await using (connector.ConfigureAwait(false))
        {
            var quaggans = await connector.GetQuaggans()
                                          .ConfigureAwait(false);

            var quagganName = quaggans[new Random(DateTime.Now.Millisecond).Next(0, quaggans.Count - 1)];

            var quagganData = await connector.GetQuaggan(quagganName)
                                             .ConfigureAwait(false);

            await commandContext.Message
                                .DeleteAsync()
                                .ConfigureAwait(false);

            await commandContext.Channel
                                .SendMessageAsync(quagganData.Url)
                                .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}