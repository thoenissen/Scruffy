using Discord.Commands;

using Scruffy.Services.Account;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("account")]
[Alias("ac")]
[BlockedChannelCheck]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
public class AccountCommandModule : LocatedCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Account administration service
    /// </summary>
    public AccountAdministrationService AdministrationService { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Adding an account
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("add")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public async Task Add()
    {
       await UserManagementService.CheckDiscordAccountAsync(Context.User.Id)
                                  .ConfigureAwait(false);

       await AdministrationService.Add(Context)
                                  .ConfigureAwait(false);
    }

    /// <summary>
    /// Editing an account
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("edit")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public async Task Edit()
    {
        await UserManagementService.CheckDiscordAccountAsync(Context.User.Id)
                                   .ConfigureAwait(false);

        await AdministrationService.Edit(Context)
                                   .ConfigureAwait(false);
    }

    /// <summary>
    /// Remove an account
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command("remove")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public async Task Remove()
    {
        await UserManagementService.CheckDiscordAccountAsync(Context.User.Id)
                                   .ConfigureAwait(false);

        await AdministrationService.Remove(Context)
                                   .ConfigureAwait(false);
    }

    #endregion // Command methods

}