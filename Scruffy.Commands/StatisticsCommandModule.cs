using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Statistics;

namespace Scruffy.Commands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("statistics")]
[Aliases("stats", "st")]
[RequireDeveloperPermissions]
[BlockedChannelCheck]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Administration)]
[ModuleLifespan(ModuleLifespan.Transient)]
public class StatisticsCommandModule : LocatedCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Visualizer
    /// </summary>
    public StatisticsVisualizerService VisualizerService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Adding a one time event
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("me")]
    [RequireGuild]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Me(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               await VisualizerService.PostMeOverview(commandContextContainer)
                                                      .ConfigureAwait(false);
                           });
    }

    #endregion // Methods
}