using Discord;
using Discord.Interactions;

using Scruffy.Services.Configuration;
using Scruffy.Services.Discord;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Server configuration commands
/// </summary>
[DefaultMemberPermissions(GuildPermission.Administrator)]
[DontAutoRegister]
public class ConfigurationSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public ConfigurationCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Server configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("configuration", "Server configuration")]
    public Task Configure() => CommandHandler.Configure(Context);

    #endregion // Methods
}