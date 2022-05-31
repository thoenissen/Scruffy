using Discord;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Administration;

/// <summary>
/// Administration service
/// </summary>
public class AdministrationCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Blocked channels
    /// </summary>
    private readonly BlockedChannelService _blockedChannelService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="blockedChannelService">Blocked channels</param>
    public AdministrationCommandHandler(LocalizationService localizationService, BlockedChannelService blockedChannelService)
        : base(localizationService)
    {
        _blockedChannelService = blockedChannelService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RenameMember(IContextContainer context, IGuildUser user, string name)
    {
        try
        {
            await user.ModifyAsync(obj => obj.Nickname = name)
                      .ConfigureAwait(false);

            await context.ReplyAsync(LocalizationGroup.GetText("ActionSuccess", "The action has been executed successfully."), ephemeral: true)
                         .ConfigureAwait(false);
        }
        catch
        {
            await context.ReplyAsync(LocalizationGroup.GetText("ActionFailed", "The execution of the action failed."), ephemeral: true)
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="context">Command context</param>
    /// <param name="role">Role</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RenameRole(IContextContainer context, IRole role, string name)
    {
        try
        {
            await role.ModifyAsync(obj => obj.Name = name)
                      .ConfigureAwait(false);

            await context.ReplyAsync(LocalizationGroup.GetText("ActionSuccess", "The action has been executed successfully."), ephemeral: true)
                         .ConfigureAwait(false);
        }
        catch
        {
            await context.ReplyAsync(LocalizationGroup.GetText("ActionFailed", "The execution of the action failed."), ephemeral: true)
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Move users to another channel
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="source">Source</param>
    /// <param name="destination">Destination</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task MoveUsers(InteractionContextContainer context, IVoiceChannel source, IVoiceChannel destination)
    {
        var message = await context.DeferProcessing()
                                   .ConfigureAwait(false);

        var moveTasks = new List<Task>();

        await foreach (var users in source.GetUsersAsync())
        {
            foreach (var user in users.Where(obj => obj.VoiceChannel?.Id == source.Id))
            {
                moveTasks.Add(context.Guild.MoveAsync(user, destination));
            }
        }

        await Task.WhenAll(moveTasks)
                  .ConfigureAwait(false);

        await message.DeleteAsync()
                     .ConfigureAwait(false);
    }

    #endregion // Methods
}