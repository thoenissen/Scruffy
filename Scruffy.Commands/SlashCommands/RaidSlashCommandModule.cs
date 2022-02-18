using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Raid;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Raid slash commands
/// </summary>
[Group("raid", "Raid commands")]
public class RaidSlashCommandModule : LocatedInteractionModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public RaidCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("join", "Join an appointment")]
    public Task Join([Summary("Name", "Name of the appointment")]string name) => CommandHandler.Join(Context, name);

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("leave", "Leave an appointment")]
    public async Task Leave([Summary("Name", "Name of the appointment")]string name)
    {
        var message = await Context.DeferProcessing()
                                   .ConfigureAwait(false);

        await CommandHandler.Leave(Context, name)
                            .ConfigureAwait(false);

        await message.DeleteAsync()
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Post guides overview
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("guides", "Show raid guides")]
    public Task Guides() => CommandHandler.Guides(Context);

    #endregion // Commands

}