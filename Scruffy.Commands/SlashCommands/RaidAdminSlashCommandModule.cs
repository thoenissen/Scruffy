using Discord;
using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Raid;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Raid slash commands
/// </summary>
[Group("raid-admin", "Raid commands")]
[DefaultPermission(false)]
public class RaidAdminSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public RaidCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Starts the setup assistant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("setup", "Starting the raid configuration")]
    public Task Setup() => CommandHandler.Setup(Context);

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("join-user", "Register a user to an appointment")]
    public Task Join([Summary("User")]IGuildUser user,
                     [Summary("Name", "Name of the appointment")]string name) => CommandHandler.Join(Context, user, name);

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("leave-user", "Deregister a user to an appointment")]
    public async Task Leave([Summary("User")]IGuildUser user,
                            [Summary("Name", "Name of the appointment")]string name)
    {
        var message = await Context.DeferProcessing(ephemeral: true)
                                   .ConfigureAwait(false);

        await CommandHandler.Leave(Context, user, name)
                            .ConfigureAwait(false);

        await message.DeleteAsync()
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("set-template", "Set raid template")]
    public Task SetTemplate([Summary("Name", "Name of the appointment")]string name) => CommandHandler.SetTemplate(Context, name);

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("set-group-count", "Set group count")]
    public async Task SetTemplate([Summary("Name", "Name of the appointment")]string name,
                                  [Summary("Count", "Group count")]int count)
    {
        await Context.Interaction
                     .DeferAsync(ephemeral: true)
                     .ConfigureAwait(false);

        await CommandHandler.SetTemplate(Context, name, count)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("commit", "Commit raid appointment")]
    public Task Commit([Summary("Name", "Name of the appointment")]string name) => CommandHandler.Commit(Context, name);

    #endregion // Commands
}