using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("gw2")]
[BlockedChannelCheck]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
public class GuildWars2CommandBuilder : LocatedCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Quaggan service
    /// </summary>
    public QuagganService QuagganService { get; set; }

    /// <summary>
    /// Guild Wars 2 update service
    /// </summary>
    public GuildWarsUpdateService GuildWarsUpdateService { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("quaggan")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Quaggan() => QuagganService.PostRandomQuaggan(Context);

    /// <summary>
    /// Next update
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("update")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Update() => GuildWarsUpdateService.PostUpdateOverview(Context);

    #endregion // Command methods
}