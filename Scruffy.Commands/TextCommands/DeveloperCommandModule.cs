using Discord.Commands;

using Scruffy.Services.Developer;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Admin commands
/// </summary>
[Group("developer")]
[Alias("dev", "de")]
[RequireContext(ContextType.Guild)]
[RequireAdministratorPermissions]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
public class DeveloperCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Developer service
    /// </summary>
    public DeveloperService DeveloperService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Set account name
    /// </summary>
    /// <param name="accountName">name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("setAccount")]
    public Task SetAccount(string accountName) => DeveloperService.SetAccount(Context, accountName);

    #endregion // Methods
}