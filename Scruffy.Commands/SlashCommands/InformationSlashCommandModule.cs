using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Information;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Shows general information about Scruffy
/// </summary>
[DontAutoRegister]
public class InformationSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public InformationCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Shows general information about Scruffy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("info", "Information about Scruffy")]
    public Task Info() => CommandHandler.Info(Context);

    #endregion // Methods
}