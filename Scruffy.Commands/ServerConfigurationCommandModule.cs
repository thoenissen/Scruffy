using Discord;
using Discord.Commands;

using Scruffy.Services.Core;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Commands;

/// <summary>
/// Configuration the server
/// </summary>
[Group("config")]
[Alias("co")]
[RequireAdministratorPermissions]
public class ServerConfigurationCommandModule : LocatedCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Prefix resolving
    /// </summary>
    public PrefixResolvingService PrefixResolvingService { get; set; }

    /// <summary>
    /// Administration service
    /// </summary>
    public AdministrationPermissionsValidationService AdministrationPermissionsValidationService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Set the server prefix
    /// </summary>
    /// <param name="prefix">Prefix</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("prefix")]
    [RequireContext(ContextType.Guild)]
    public async Task SetPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix) == false
         && prefix.Length > 0
         && prefix.Any(char.IsControl) == false)
        {
            PrefixResolvingService.AddOrRefresh(Context.Guild.Id, prefix);

            await Context.Message
                         .ReplyAsync(LocalizationGroup.GetFormattedText("UsingNewPrefix", "I will use the following prefix: {0}", prefix))
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Set the server administration role
    /// </summary>
    /// <param name="role">Roles</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("adminRole")]
    [RequireContext(ContextType.Guild)]
    public async Task SetAdministrationRole(IRole role)
    {
        AdministrationPermissionsValidationService.AddOrRefresh(Context.Guild.Id, role.Id);

        await Context.Message
                     .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
                     .ConfigureAwait(false);
    }
    #endregion // Methods
}