using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Statistics;

namespace Scruffy.Commands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("information")]
[Aliases("info", "i")]
[ModuleLifespan(ModuleLifespan.Transient)]
public class InformationCommandModule : LocatedCommandModuleBase
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
    [GroupCommand]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public Task Info(CommandContext commandContext)
    {
        return InvokeAsync(commandContext,
                           async commandContextContainer =>
                           {
                               var build = new DiscordEmbedBuilder()
                                           .WithColor(DiscordColor.Green)
                                           .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64")
                                           .WithTimestamp(DateTime.Now)
                                           .WithTitle(LocalizationGroup.GetText("GeneralInformationTitle", "General information"))
                                           .WithDescription(LocalizationGroup.GetFormattedText("GeneralInformationDescription", "All data collected by {0} is used only for the administration of the guild. The individual areas where data is collected are discussed below.", commandContextContainer.Client.CurrentUser.Mention))
                                           .AddField(LocalizationGroup.GetText("GuildWars2ApiTitle", "Guild Wars 2 - API-Token"), LocalizationGroup.GetFormattedText("GuildWars2ApiDescription", "The following data is determined via the specified API token:\n\n - With how many characters is the guild represented?\n - When was the last login?\n - Which server is the account assigned to?\n\nAll other data is only determined temporarily when you request {0} to execute a command. You only ever have access to your own API token.", commandContextContainer.Client.CurrentUser.Mention))
                                           .AddField(LocalizationGroup.GetText("DiscordTitle", "Discord"), LocalizationGroup.GetFormattedText("DiscordDescription", "Part of the guild ranking system is the activity in Discord. For this, how many messages are written and how many minutes you are in the voice chat are saved. The content of the posts is irrelevant and is not saved.", commandContextContainer.Client.CurrentUser.Mention))
                                           .AddField(LocalizationGroup.GetText("RefreshTitle", "State"), LocalizationGroup.GetFormattedText("RefreshDescription", "As of: {0}", new DateTime(2021, 10, 17).ToString("d", LocalizationGroup.CultureInfo)));

                               await commandContextContainer.Channel
                                                            .SendMessageAsync(build)
                                                            .ConfigureAwait(false);
                           });
    }

    #endregion // Methods
}