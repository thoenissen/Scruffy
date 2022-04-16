using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Information;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Shows general informations about Scruffy
/// </summary>
public class InformationSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Visualizer
    /// </summary>
    public InformationCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Shows general informations about Scruffy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("info", "Display infos about Scruffy")]
    public Task Info() => CommandHandler.Info(Context);

    #endregion // Methods
}