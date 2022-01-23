using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("gw2")]
[ModuleLifespan(ModuleLifespan.Transient)]
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
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("quaggan")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Quaggan(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               await QuagganService.PostRandomQuaggan(commandContextContainer)
                                                   .ConfigureAwait(false);
                           });
    }

    /// <summary>
    /// Next update
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("update")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Update(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           commandContextContainer => GuildWarsUpdateService.PostUpdateOverview(commandContextContainer));
    }

    #endregion // Command methods
}