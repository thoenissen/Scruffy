using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Guild Wars 2 commands
/// </summary>
[Group("gw2", "Guild Wars 2 related commands")]
public class GuildWars2SlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GuildWars2CommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("quaggan", "Posting random quaggan GIFs")]
    public Task Quaggan() => CommandHandler.Quaggan(Context);

    /// <summary>
    /// Next update
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("update", "Display information about the next Guild Wars 2 update")]
    public Task Update() => CommandHandler.Update(Context);

    #endregion // Command methods
}